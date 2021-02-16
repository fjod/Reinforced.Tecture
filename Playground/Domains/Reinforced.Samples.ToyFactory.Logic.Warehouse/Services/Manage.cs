﻿using System;
using System.Linq;
using Reinforced.Samples.ToyFactory.Logic.Channels;
using Reinforced.Samples.ToyFactory.Logic.Channels.Queries;
using Reinforced.Samples.ToyFactory.Logic.Warehouse.Entities;
using Reinforced.Tecture.Aspects.Orm.Commands.Add;
using Reinforced.Tecture.Aspects.Orm.Commands.Delete;
using Reinforced.Tecture.Aspects.Orm.Commands.DeletePk;
using Reinforced.Tecture.Aspects.Orm.Commands.Update;
using Reinforced.Tecture.Aspects.Orm.Commands.UpdatePk;
using Reinforced.Tecture.Aspects.Orm.PrimaryKey;
using Reinforced.Tecture.Aspects.Orm.Queries;
using Reinforced.Tecture.Aspects.Orm.Toolings;
using Reinforced.Tecture.Commands;
using Reinforced.Tecture.Services;

namespace Reinforced.Samples.ToyFactory.Logic.Warehouse.Services
{
    public class Manage : TectureService<
        Adds<Resource>,
        Modifies<MeasurementUnit>
    >
    
    {
        public IAddition<MeasurementUnit> CreateMeasurementUnit(string name, string shortName)
        {
            if (From<Db>().Get<MeasurementUnit>().All
                .Describe("check unit existence")
                .Any(x => x.Name == name || x.ShortName == shortName))
            {
                throw new Exception($"Cannot add measurement unit '{name}' because it already exists");
            }

            return To<Db>().Add(new MeasurementUnit() //!!!
            {
                Name = name,
                ShortName = shortName
            })
                .Annotate($"create measurement unit '{name}' ({shortName})");
        }
        
        private Manage() { }

        public void RenameMeasurementUnit(int id, string name, string shortName)
        {
            To<Db>().Update<MeasurementUnit>()
                .Set(x => x.Name, name)
                .Set(x => x.ShortName, shortName)
                .ByPk(id);
        }

        public void DeleteMeasurementUnit(int id)
        {
            var mu = From<Db>().Get<MeasurementUnit>().ById(id);
            To<Db>().Delete<MeasurementUnit>().ByPk(id).Annotate($"remove measurement unit#{id}");
        }

        public IAddition<Resource> CreateResource(string name, string measurementUnit)
        {
            if (From<Db>().Get<Resource>().All.Describe("check resource existence").Any(x => x.Name == name))
            {
                throw new Exception($"Cannot add resource '{name}' because it already exists");
            }

            var unit = From<Db>().All<MeasurementUnit>()
                .Describe($"lookup measurement unit by name ({measurementUnit})")
                .Where(x => x.Name == measurementUnit || x.ShortName == measurementUnit)
                .Select(x => x.Id)
                .First();

            var res = new Resource() { Name = name, MeasurementUnitId = unit };
            var ex = To<Db>().Add(res).Annotate($"new resource {name}");
            return ex;
        }
    }
}
