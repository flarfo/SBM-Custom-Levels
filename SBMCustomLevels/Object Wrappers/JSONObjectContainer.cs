using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SBM_CustomLevels.ObjectWrappers
{
    [Serializable]
    public class JSONObjectContainer
    {
        public string Version = "1.4";
        public static Dictionary<string, string[]> VersionCompatibility = new Dictionary<string, string[]>(){
            { "1.4", new string[] {"1.4"} }, 
        };

        public int worldType;

        public FloatObject spawnPosition1;
        public FloatObject spawnPosition2;
        public FloatObject spawnPosition3;
        public FloatObject spawnPosition4;

        [JsonConverterAttribute(typeof(SBMObjectConverter))]
        public Dictionary<int, DefaultObject> objects = new Dictionary<int, DefaultObject>();

        [JsonConstructor]
        public JSONObjectContainer()
        {

        }

        // Version 1.4+
        public JSONObjectContainer(int _worldType, FloatObject pos1, FloatObject pos2, FloatObject pos3, FloatObject pos4, Dictionary<int, DefaultObject> _objects)
        {
            worldType = _worldType;

            spawnPosition1 = pos1;
            spawnPosition2 = pos2;
            spawnPosition3 = pos3;
            spawnPosition4 = pos4;

            objects = _objects;
        }
    }

    public class SBMObjectConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DefaultObject);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<int, DefaultObject>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject) break;

                int dictKey = int.Parse((string)reader.Value);
                reader.Read(); // progress to next json value (object at dictKey)

                JObject jo = JObject.Load(reader);
                var sbmObjectType = (ObjectType)jo["objectType"].Value<int>();
                
                switch (sbmObjectType)
                {
                    case ObjectType.Default:
                        result.Add(dictKey, jo.ToObject<DefaultObject>(serializer));
                        break;
                    case ObjectType.ColorBlock:
                        result.Add(dictKey, jo.ToObject<ColorBlockObject>(serializer));
                        break;
                    case ObjectType.FlipBlock:
                        result.Add(dictKey, jo.ToObject<FlipBlockObject>(serializer));
                        break;
                    case ObjectType.MeshSlice:
                        result.Add(dictKey, jo.ToObject<MeshSliceObject>(serializer));
                        break;
                    case ObjectType.Piston:
                        result.Add(dictKey, jo.ToObject<PistonObject>(serializer));
                        break;
                    case ObjectType.Rail:
                        result.Add(dictKey, jo.ToObject<RailObject>(serializer));
                        break;
                    case ObjectType.SeeSaw:
                        result.Add(dictKey, jo.ToObject<SeeSawObject>(serializer));
                        break;
                    case ObjectType.Spline:
                        result.Add(dictKey, jo.ToObject<SplineObject>(serializer));
                        break;
                    case ObjectType.Water:
                        result.Add(dictKey, jo.ToObject<WaterObject>(serializer));
                        break;
                    default:
                        result.Add(dictKey, jo.ToObject<DefaultObject>(serializer));
                        break;
                }
            }

            return result;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
