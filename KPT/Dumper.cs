﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace KPT
{
    class Dumper
    {

        private const string originalDirectory = "Original";
        private const string editableDirectory = "Editable";

        /// <summary>
        /// Used by recursive function PopulateFileList to signal early termination should happpen
        /// </summary>
        private bool fileListPopulateFailed = false;

        struct FileLocationMeta
        {
            public string leadingPath;
            public string switchPath;
            public string subPath;
            public string fileName;       
            
            public FileLocationMeta(string leadingPath, string switchPath, string subPath, string fileName)
            {
                this.leadingPath = leadingPath;
                this.switchPath = switchPath;
                this.subPath = subPath;
                this.fileName = fileName;
            }
                     
        }

        /// <summary>
        /// Check if the directory is valid, that is, that it exists and that it passes any other tests specified in remarks
        /// </summary>
        /// <param name="targetDirectoryPath">The path of the directory to be checked</param>
        /// <returns>True if valid, false if invalid</returns>
        /// <remarks>
        /// Current tests are:
        /// - Directory exists
        /// - Directory contains a "PSP_GAME" folder
        /// </remarks>
        private bool CheckDirectoryValidity(string targetDirectoryPath)
        {
            if (!Directory.Exists(targetDirectoryPath))
            {
                string errorMessage = string.Format("Could not find directory {0}. Please check that the path is correct and that the directory exists.", targetDirectoryPath);
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            var initialDirectoryListing = Directory.GetDirectories(targetDirectoryPath);
            var initialDirectoryContents = new HashSet<string>();

            foreach (var dir in initialDirectoryListing)
            {
                initialDirectoryContents.Add(Path.GetFileName(dir));
            }

            if (!initialDirectoryContents.Contains("PSP_GAME"))
            {
                string errorMessage = string.Format("Directory {0} did not match the expected format. Expected a directory containing a \"PSP_GAME\" directory.", targetDirectoryPath); // this is to nudge people in the right path and be disabled or customized if necessary should another format come into play
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Go through a directory and split it into an "Original" directory with a copy of the original files and an "Editble" directory with processed files that can be edited by users
        /// </summary>
        /// <param name="sourceDirectoryPath">The path of the directory to be processed</param>
        /// <param name="targetDirectoryPath">The path of the directory to store the processed contents</param>
        /// <returns>True is succesfully processed, false if processing failed</returns>
        public bool ProcessDirectory(string sourceDirectoryPath, string targetDirectoryPath)
        {
            
            if (!CheckDirectoryValidity(sourceDirectoryPath))
            {
                return false;
            }

            List<FileLocationMeta> fileList = new List<FileLocationMeta>();

            if (!PopulateFileList(sourceDirectoryPath, fileList))
            {
                return false;
            }

            string originalFilesDirectory = Path.Combine(targetDirectoryPath, originalDirectory);
            string editableFilesDirectory = Path.Combine(targetDirectoryPath, editableDirectory);

            if (!DebugSettings.IGNORE_DIRECTORY_ALREADY_EXISTS_WARNING && Directory.Exists(originalFilesDirectory))
            {
                string errorMessage = string.Format("Directory {0} was specified as a target directory but it already has files in it.\r\n\r\nSelecting it as the target direcotry would destroy all files in the directory.\r\n\r\nIf this is your intention, please confirm by deleting the directory yourself and trying again.", targetDirectoryPath);
                MessageBox.Show(errorMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
                //Directory.Delete(originalFilesDirectory, true); // there needs to be some "type in that you actually want to over write these files" prompt before this can be allowed in live versions - actually see above 
            }

            if (!DebugSettings.IGNORE_DIRECTORY_ALREADY_EXISTS_WARNING && Directory.Exists(editableFilesDirectory))
            {
                string errorMessage = string.Format("Directory {0} was specified as a target directory but it already has files in it.\r\n\r\nSelecting it as the target direcotry would destroy all files in the directory.\r\n\r\nIf this is your intention, please confirm by deleting the directory yourself and trying again.", targetDirectoryPath);
                MessageBox.Show(errorMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (!DebugSettings.SKIP_ORIGINAL_DIRECTORY_COPYING)
            {
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sourceDirectoryPath, originalFilesDirectory);
            }

            

            foreach (var file in fileList)
            {
                HandleFile(file);
            }

            return true;
        }

        

        private void HandleFile(FileLocationMeta file)
        {
            
        }

        /// <summary>
        /// Iterate recursively through a given directory and its subdirectories and fill the provided list with information on all files contained within
        /// </summary>
        /// <param name="leadingPath">The directory to start from</param>
        /// <param name="fileList">The file list which should be populated</param>
        private bool PopulateFileList(string leadingPath, List<FileLocationMeta> fileList)
        {
            return PopulateFileList(leadingPath, "", fileList);
        }

        /// <summary>
        /// Iterate recursively through a given directory and its subdirectories and fill the provided list with information on all files contained within
        /// </summary>
        /// <param name="leadingPath">The directory to start from</param>
        /// <param name="subPath">Used to keep track of the current subdirectory that is being iterated though (usually set to "" initially)</param>
        /// <param name="fileList">The file list which should be populated</param>
        private bool PopulateFileList(string leadingPath, string subPath, List<FileLocationMeta> fileList)
        {

            if (fileListPopulateFailed) // try to quit as fast as possible if there has been an exception
            {
                return false;
            }

            string currentAbsolutePath = Path.Combine(leadingPath, subPath);

            try
            {               
                var subdirectories = Directory.GetDirectories(currentAbsolutePath);

                foreach (var subdirectory in subdirectories)
                {
                    string dirWithoutPath = Path.GetFileName(subdirectory);
                    string newSubPath = Path.Combine(subPath, dirWithoutPath);
                    PopulateFileList(leadingPath, newSubPath, fileList);
                }

                var currentDirectoryFiles = Directory.GetFiles(currentAbsolutePath);

                foreach (var file in currentDirectoryFiles)
                {
                    string filenameWithoutPath = Path.GetFileName(file);
                    FileLocationMeta newFileData = new FileLocationMeta();
                    newFileData.leadingPath = leadingPath;
                    newFileData.subPath = subPath;
                    newFileData.fileName = filenameWithoutPath;
                    fileList.Add(newFileData);
                }

                return true;

            }
            catch (Exception e) {

                if (fileListPopulateFailed) // don't bother showing more than one exception message
                {
                    fileListPopulateFailed = true;
                    string errorMessage = string.Format("There was an error while looking for files in {0}.\r\n\r\n{1}", currentAbsolutePath, e.Message);
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return false;

            }

        }

    }
}