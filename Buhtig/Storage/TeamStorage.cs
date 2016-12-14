using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Buhtig.Entities.User;
using Newtonsoft.Json;

namespace Buhtig.Storage
{
    [Serializable, JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TeamStorage
    {
        public ObservableCollection<Team> Teams { get; set; }

        public TeamStorage()
        {
            Teams = new ObservableCollection<Team>();
        }

        public TeamStorage(Stream jsonStream)
        {
            var reader = new StreamReader(jsonStream);
            var serializer = new JsonSerializer();
            var teams = serializer.Deserialize(reader, typeof(IEnumerable<Team>)) as IEnumerable<Team>;
            if (teams == null) throw new JsonReaderException("Nothing Deserialized.");
            reader.Close();
            Teams = new ObservableCollection<Team>(teams);
            foreach (var team in Teams)
            {
                team.PostProcess();
            }
        }

        public void Save(Stream jsonStream)
        {
            var writer = new StreamWriter(jsonStream);
            var teamsJson = JsonConvert.SerializeObject(Teams);
            writer.Write(teamsJson);
            writer.Close();
        }
    }
}
