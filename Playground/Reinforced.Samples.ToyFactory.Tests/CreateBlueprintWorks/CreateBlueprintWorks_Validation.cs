using System;
using Reinforced.Tecture.Testing.Validation;
using Reinforced.Tecture.Tracing;
using Reinforced.Samples.ToyFactory.Logic.Entities;
using Reinforced.Tecture.Features.Orm.Commands.Add;
using Reinforced.Tecture.Features.Orm.Commands.Relate;
using static Reinforced.Tecture.Features.Orm.Testing.Checks.Add.AddChecks;
using static Reinforced.Tecture.Testing.BuiltInChecks.CommonChecks;

namespace Reinforced.Samples.ToyFactory.Tests.CreateBlueprintWorks
{
		class CreateBlueprintWorks_Validation : ValidationBase
		{
			protected override void Validate(TraceValidator flow)
			{ 
				flow.Then<Add<Blueprint>>
				(
					Add<Blueprint>(x=>
					{ 
						if (x.ToyTypeId != 79) return false;
						return true;
					}, @"Create blueprint"), 
					Annotated(@"Create blueprint")
				);
				flow.Then<Relate>
				(
					Annotated(@"Ensure association with toy type")
				);
				flow.Then<Save>
				(
					Saved(), 
					Annotated(@"")
				);
				flow.TheEnd();
			}

		}
}
