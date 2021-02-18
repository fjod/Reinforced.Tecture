﻿using System;
using System.Collections.Generic;
using System.Linq;
using Reinforced.Samples.ToyFactory.Logic.Channels;
using Reinforced.Samples.ToyFactory.Logic.Channels.Queries;
using Reinforced.Samples.ToyFactory.Logic.Warehouse.Dto;
using Reinforced.Samples.ToyFactory.Logic.Warehouse.Entities;
using Reinforced.Samples.ToyFactory.Logic.Warehouse.Entities.Suppliement;
using Reinforced.Tecture.Aspects.DirectSql;
using Reinforced.Tecture.Aspects.DirectSql.Commands;
using Reinforced.Tecture.Aspects.DirectSql.Queries;
using Reinforced.Tecture.Aspects.DirectSql.Toolings;
using Reinforced.Tecture.Aspects.Orm.Commands.Add;
using Reinforced.Tecture.Aspects.Orm.Commands.Relate;
using Reinforced.Tecture.Aspects.Orm.PrimaryKey;
using Reinforced.Tecture.Aspects.Orm.Queries;
using Reinforced.Tecture.Aspects.Orm.Toolings;
using Reinforced.Tecture.Commands;
using Reinforced.Tecture.Services;

namespace Reinforced.Samples.ToyFactory.Logic.Warehouse.Services
{
    public class SupplyService : 
        TectureService<
            Adds<Resource,ResourceSupply,ResourceSupplyItem>,
            Updates<ResourceSupplyItem>,
            MakesSqlCommands
        >
    {
        private SupplyService() { }

        public void RemoveResourceSupply(int id)
        {
            From<Db>().All<ResourceSupply>().EnsureExists(id);

            To<Db>().Sql<ResourceSupplyItem>(x => $"DELETE {x.Alias()} FROM {x} WHERE {x.ResourceSupplyId == id}");
            To<Db>().Sql<ResourceSupply>(x => $"DELETE {x.Alias()} FROM {x} WHERE {x.Id == id}");
        }

        public void FinishResourceSupply(int id)
        {
            To<Db>().Sql<Resource, ResourceSupplyItem>((res, item) => $@"
    UPDATE {res.Alias()}
    SET {res.StockQuantity == res.StockQuantity + item.Quantity}
    FROM {res}
    INNER JOIN {item} ON {item.ResourceId == res.Id}
    WHERE {item.ResourceSupplyId==id}
");
            To<Db>().Sql<ResourceSupply>(r =>
                $"UPDATE {r.Alias()} SET {r.Status == ResourceSupplyStatus.Closed} FROM {r} WHERE {r.Id == id}");
        }

        private void UpdateResourceSupplyItemsCount(int supplyId)
        {
            Final.ContinueWith(() =>
            {
                To<Db>().Sql<ResourceSupply, ResourceSupplyItem>((r, item) =>
                    $"UPDATE {r.Alias()} SET {r.ItemsCount} = (SELECT COUNT(*) FROM {item} WHERE {item.ResourceSupplyId == supplyId}) FROM {r}");
            });
            
        }

        /// <summary>
        /// Creates or adds supply of some resources
        /// </summary>
        /// <param name="name">Name of the supply operation (something to distinguish it from other similar calls; for user)</param>
        /// <param name="items">Items to be created/added in db</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IAddition<ResourceSupply> CreateResourceSupply(string name, IEnumerable<ResourceItemDto> items)
        {
            var rs = new ResourceSupply //create db entity for the operation; items count will be set in the the end
            {
                CreationDate = DateTime.UtcNow,
                Name = name,
                Status = ResourceSupplyStatus.Open
            };
            var r = To<Db>().Add(rs).Annotate("add resource supply"); //add record of resource supply to db
            var toBeAdded = items.ToArray();
            //search if there is already such items in db
            var knownIds = toBeAdded.//items with ID (so we should already have them in db)
                Where(x => x.Id.HasValue).
                Select(x => x.Id.Value).
                ToArray();
            var names = toBeAdded.//items without IDs - new items for us
                Where(x => !x.Id.HasValue).
                Select(x => x.Name).
                ToArray();

            var byNames = From<Db>().All<Resource>() //get names from db which are present in new supply
                    .Describe($"lookup resources by {names.Length} names")
                    .Where(x => names.Contains(x.Name));

            var byIds = From<Db>().SqlQuery<Resource> //get Id's from db which are present in new supply?
                    (x => $"SELECT * FROM {x} WHERE {knownIds.Contains(x.Id)}").As<Resource>();

            var byNamesDict = byNames.ToDictionary(x => x.Name);
            var byIdsSet = new HashSet<int>(byIds.Select(x => x.Id));

            using (var c = Cycle("add items to supply")) //creates cycle of addition 
            {
                foreach (var resourceItemDto in toBeAdded)
                {
                    int resourceId; //need to find item's Id 
                    if (resourceItemDto.Id.HasValue) //if it's already present simply validate it
                    {
                        Comment($"looking resource by id {resourceItemDto.Id}");
                        if (!byIdsSet.Contains(resourceItemDto.Id.Value))
                        {
                            throw new Exception($"could not locate resource with id#{resourceItemDto.Id.Value}");
                        }

                        resourceId = resourceItemDto.Id.Value;
                    }
                    else //if it's not present
                    {
                        if (!byNamesDict.ContainsKey(resourceItemDto.Name))
                        {
                            throw new Exception($"could not locate resource with name '{resourceItemDto.Name}'");
                        }

                        resourceId = byNamesDict[resourceItemDto.Name].Id; //take it from resource object after validation
                    }

                    var rsi = new ResourceSupplyItem() //construct new record of supply
                    {
                        ResourceId = resourceId, //id of resource
                        Quantity = resourceItemDto.Quantity //amount of resource
                    };

                    To<Db>().Relate(rsi, x => x.ResourceSupply, rs); //add FK in joining table
                                                                    // Resources <-- ResourceSupplyItems -> ResourceSupplies
                    To<Db>().Add(rsi).Annotate("add resource supply item"); //finally add record to db 
                    c.Iteration("added resource item"); //add information for debug purposes
                }
            }

            Save.ContinueWith(() => //Action after save
            {
                var id = From<Db>().Key(r); //get id of this ResourceSupply
                UpdateResourceSupplyItemsCount(id); //and update it's item count
                FinishResourceSupply(id);
            });
            return r;

        }
    }
}