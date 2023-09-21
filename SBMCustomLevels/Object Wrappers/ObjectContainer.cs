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
        public FloatObject spawnPosition3;
        public FloatObject spawnPosition4;

        public List<DefaultObject> defaultObjects;
        public List<WaterObject> waterObjects;
        public List<MeshSliceObject> meshSliceObjects;
        public List<FlipBlockObject> flipBlockObjects;
        public List<PistonObject> pistonObjects;
        public List<RailObject> railObjects;
        public List<SplineObject> splineObjects;
        public List<ColorBlockObject> colorBlockObjects;

        [JsonConstructor]
        public ObjectContainer()
        {

        }

        public ObjectContainer(FloatObject pos1, FloatObject pos2, FloatObject pos3, FloatObject pos4, List<DefaultObject> _defaultObjects, List<WaterObject> _waterObjects, 
            List<MeshSliceObject> _meshSliceObjects, List<FlipBlockObject> _flipBlockObjects, List<PistonObject> _pistonObjects, 
            List<RailObject> _railObjects, List<SplineObject> _splineObjects, List<ColorBlockObject> _colorBlockObjects)
        {
            spawnPosition1 = pos1;
            spawnPosition2 = pos2;
            spawnPosition3 = pos3;
            spawnPosition4 = pos4;

            defaultObjects = _defaultObjects;
            waterObjects = _waterObjects;
            meshSliceObjects = _meshSliceObjects;
            flipBlockObjects = _flipBlockObjects;
            pistonObjects = _pistonObjects;
            railObjects = _railObjects;
            splineObjects = _splineObjects;
            colorBlockObjects = _colorBlockObjects;
        }
    }
}
