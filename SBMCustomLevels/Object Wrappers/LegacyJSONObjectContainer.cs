using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SBM_CustomLevels.Objects;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class LegacyJSONObjectContainer
    {
        // Anything before 1.4 should be compatible with 1.3
        public static string Version = "1.3";
        public static string[] VersionCompatibility = { "1.3" };

        public FloatObject spawnPosition1;
        public FloatObject spawnPosition2;
        public FloatObject spawnPosition3;
        public FloatObject spawnPosition4;

        public List<DefaultObject> defaultObjects;
        public List<WaterObject> waterObjects;
        public List<MeshSliceObject> meshSliceObjects;
        public List<SeeSawObject> seeSawObjects;
        public List<FlipBlockObject> flipBlockObjects;
        public List<PistonObject> pistonObjects;
        public List<RailObject> railObjects;
        public List<SplineObject> splineObjects;
        public List<ColorBlockObject> colorBlockObjects;

        [JsonConstructor]
        public LegacyJSONObjectContainer()
        {

        }

        // Legacy constructor, in case of old file type
        public LegacyJSONObjectContainer(FloatObject pos1, FloatObject pos2, FloatObject pos3, FloatObject pos4, List<DefaultObject> _defaultObjects, List<WaterObject> _waterObjects,
            List<MeshSliceObject> _meshSliceObjects, List<SeeSawObject> _seeSawObjects, List<FlipBlockObject> _flipBlockObjects, List<PistonObject> _pistonObjects,
            List<RailObject> _railObjects, List<SplineObject> _splineObjects, List<ColorBlockObject> _colorBlockObjects)
        {
            spawnPosition1 = pos1;
            spawnPosition2 = pos2;
            spawnPosition3 = pos3;
            spawnPosition4 = pos4;

            defaultObjects = _defaultObjects;
            waterObjects = _waterObjects;
            meshSliceObjects = _meshSliceObjects;
            seeSawObjects = _seeSawObjects;
            flipBlockObjects = _flipBlockObjects;
            pistonObjects = _pistonObjects;
            railObjects = _railObjects;
            splineObjects = _splineObjects;
            colorBlockObjects = _colorBlockObjects;
        }
    }
}

