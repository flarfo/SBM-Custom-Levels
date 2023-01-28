using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SBM_CustomLevels
{
    [Serializable]
    public class ObjectContainer
    {
        public FloatObject spawnPosition1;
        public FloatObject spawnPosition2;

        public List<DefaultObject> defaultObjects;
        public List<WaterObject> waterObjects;
        public List<RailObject> railObjects;

        [JsonConstructor]
        public ObjectContainer()
        {

        }

        public ObjectContainer(FloatObject pos1, FloatObject pos2, List<DefaultObject> _defaultObjects, List<WaterObject> _waterObjects, List<RailObject> _railObjects)
        {
            spawnPosition1 = pos1;
            spawnPosition2 = pos2;
            defaultObjects = _defaultObjects;
            waterObjects = _waterObjects;
            railObjects = _railObjects;
        }
    }
}
