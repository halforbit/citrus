
namespace Citrus.Web.Service
{
    using System.Linq;

    using Citrus;

    [Controller]
    public class ServiceController
    {
        public object Get()
        {
            // GET /

            return "Welcome Home!";
        }

        public class Vehicles
        {
            public object Get()
            {
                // GET /vehicles

                return Data.TestDatastore.Instance
                    .Select(v => new
                    {
                        VehicleId = v.Key,

                        Make = v.Value.Make,

                        Model = v.Value.Model
                    })
                    .ToArray();
            }

            public class Add
            {
                public void Get()
                {
                    // GET /vehicles/add
                }

                public void Post(dynamic body)
                {
                    // POST /vehicles/add

                    var newId = Data.TestDatastore.Instance.Keys.Max() + 1;

                    Data.TestDatastore.Instance.TryAdd(
                        newId,
                        new Model.Vehicle
                        {
                            Make = body.make,

                            Model = body.model
                        });
                }   
            }

            public class _VehicleId
            {
                public object Get(int vehicleId)
                {
                    // GET /vehicles/_vehicle-id

                    return Data.TestDatastore
                        .Instance
                        .Select(v => new
                        {
                            VehicleId = v.Key,

                            Make = v.Value.Make,

                            Model = v.Value.Model
                        })
                        .FirstOrDefault(v => v.VehicleId == vehicleId);
                }

                public void Delete(int vehicleId)
                {
                    // DELETE /vehicles/_vehicle-id

                    var vehicle = null as Model.Vehicle;

                    Data.TestDatastore.Instance.TryRemove(vehicleId, out vehicle);
                }

                public class Edit
                {
                    public object Get(int vehicleId)
                    {
                        // GET /vehicles/_vehicle-id/edit

                        return Data.TestDatastore
                            .Instance
                            .Select(v => new
                            {
                                VehicleId = v.Key,

                                Make = v.Value.Make,

                                Model = v.Value.Model
                            })
                            .FirstOrDefault(v => v.VehicleId == vehicleId);
                    }

                    public void Post(int vehicleId, dynamic body)
                    {
                        // POST /vehicles/_vehicle-id/edit

                        var vehicle = Data.TestDatastore.Instance[vehicleId];

                        vehicle.Make = body.make;

                        vehicle.Model = body.model;
                    }
                }
            }
        }
    }
}