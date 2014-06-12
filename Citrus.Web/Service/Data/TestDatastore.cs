
namespace Citrus.Web.Service.Data
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class TestDatastore : ConcurrentDictionary<int, Model.Vehicle>
    {
        public TestDatastore()
            : base(new Dictionary<int, Model.Vehicle>
            {
                { 123, new Model.Vehicle { Make = "Ford", Model = "Focus" } },

                { 124, new Model.Vehicle { Make = "Ford", Model = "Mustang" } },

                { 125, new Model.Vehicle { Make = "Ford", Model = "Fusion" } },

                { 126, new Model.Vehicle { Make = "Kia", Model = "Optima" } }
            })
        {

        }

        static TestDatastore()
        {
            Instance = new TestDatastore();
        }

        public static TestDatastore Instance { get; set; }
    }
}