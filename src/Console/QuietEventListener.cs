﻿using System;
using Fettle.Core;

namespace Fettle.Console
{
    internal class QuietEventListener : IEventListener
    {
        private readonly IOutputWriter outputWriter;
        private bool isFirstFile;

        public QuietEventListener(IOutputWriter outputWriter)
        {
            this.outputWriter = outputWriter;
        }

        public void BeginCoverageAnalysisOfTestCase(string fullTestName, int index, int total)
        {
        }

        public void BeginMutationOfFile(string filePath, string baseSourceDirectory, int index, int total)
        {
            if (isFirstFile)
            {
                outputWriter.Write(Environment.NewLine);
                isFirstFile = false;
            }
        }

        public void MemberMutating(string name)
        {            
        }

        public void SyntaxNodeMutating(int index, int total)
        {
        }

        public void MutantSurvived(SurvivingMutant survivor)
        {
        }

        public void EndMutationOfFile(string filePath)
        {
            outputWriter.Write(".");
        }
    }
}