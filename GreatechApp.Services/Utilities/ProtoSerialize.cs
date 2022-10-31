namespace GreatechApp.Services.Utilities
{
    using System.IO;
    using ProtoBuf;

    public class ProtoSerialize
    {
        #region Variable
        readonly string directory = @"..\Config Section\Data";
        readonly object Serialize_lock = new object();
        readonly object Deserialize_lock = new object();
        #endregion

        #region Constructor
        public ProtoSerialize()
        {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
        #endregion

        #region Method
        public void SerializeData<T>(T record, string FileName) where T : class
        {
            lock (Serialize_lock)
            {
                using (var outputFile = File.Create(directory + Path.DirectorySeparatorChar + FileName))
                {
                    Serializer.Serialize(outputFile, record);
                }
            }

        }

        public T DeserializeData<T>(string FileLocation) where T : class
        {
            lock (Deserialize_lock)
            {
                if (File.Exists(directory + Path.DirectorySeparatorChar + FileLocation))
                {
                    using (var inputFile = File.OpenRead(directory + Path.DirectorySeparatorChar + FileLocation))
                    {
                        return Serializer.Deserialize<T>(inputFile);
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
    }
}
