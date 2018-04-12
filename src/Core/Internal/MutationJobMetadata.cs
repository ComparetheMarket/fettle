namespace Fettle.Core.Internal
{
    internal class MutationJobMetadata
    {
        public string SourceFilePath { get; set;  }
        public int SourceFileIndex { get; set;  }
        public int SourceFilesTotal { get; set;  }

        public string MethodName { get; set;  }

        public int SyntaxNodeIndex { get; set;  }
        public int SyntaxNodesTotal { get; set;  }
    }
}