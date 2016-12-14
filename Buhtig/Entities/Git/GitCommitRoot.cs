using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buhtig.Resources.Strings;

namespace Buhtig.Entities.Git
{
    public class GitCommitRoot : GitCommit
    {
        public GitCommitRoot()
        {
            Sha = StaticConfigs.CommitRootSha;
            ShaShort = Sha.Substring(0, 7);
            Time = DateTime.MinValue;
            TimeOriginal = Time;
            Author = null;
            AuthorId = Guid.Empty;
            MessageShort = Language.CommitRootMessage;
            Message = Language.CommitRootMessage;
            ChangeShas = new Dictionary<string, List<GitChange>>();
            Changes = new Dictionary<GitCommit, List<GitChange>>();
            ChangesList = new ObservableCollection<GitChange>();
            ChildrenSha = new ObservableCollection<string>();
            Children = new ObservableCollection<GitCommit>();
            InnerCommit = null;
            TimeEstimated = false;
            MinTime = Time;
            MaxTime = Time;
        }
    }
    public static class RuntimeRoot
    {
        public static GitCommitRoot CommitRoot = new GitCommitRoot();
    }
}
