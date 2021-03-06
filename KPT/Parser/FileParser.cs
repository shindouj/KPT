﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using KPT.Parser.Elements;
using KPT.Parser.Instructions;
using KPT.Parser.Headers;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace KPT.Parser
{
    /// <summary>
    /// Represents a processed file
    /// </summary>
    class KCFile
    {
        public IHeader header;
        public List<IInstruction> instructions;
        public DataBox footer; // The footer appears to be some kind of padding of 0x88, so it is represented with a Box instead of a specific footer object
    }
    // perhaps FileParser and KCFile should be merged?

    class FileParser
    {

        public static bool IsParseable(string fileName)
        {
            Regex regex = new Regex(@"St[0-9A-F]{3}_Cp[0-9A-F]{4}\.bin"); // search for the StXXX_CpXXXX.bin format. since that's the only file we can parse at the moment there's not much detail to go into to determine whether or not it is parseable.
            Match match = regex.Match(fileName);
            return match.Success;
        }

        public FileParser()
        {

        }

        /// <summary>
        /// A helper function for creating Boxes
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        /// <remarks>
        /// Intended to make Box creating code cleaner by delegating the getting of instruction size and construction of object to a seperate function
        /// </remarks>
        private InstructionBox MakeInstructionBox(Opcode opcode)
        {
            int instructionSize = OpcodeInfo.GetInstructionSize(opcode);
            return new InstructionBox(instructionSize);
        }

        /// <summary>
        /// Takes a file and breaks it down into IElements then returns it in a structured format
        /// </summary>
        /// <param name="br">The file to process</param>
        /// <param name="fileName">The name of the file to be processed (used only for displaying error messages)</param>
        /// <returns>A class containing the processed file data</returns>
        public KCFile ParseFile(BinaryReader br, string fileName)
        {
            KCFile workingFile = new KCFile();
            List<IInstruction> instructions = new List<IInstruction>();

            DataBox footer = ReadFooter(br);
            workingFile.footer = footer;
            workingFile.header = ReadHeader(br);

            long streamEnd = br.BaseStream.Length - footer.GetContentsSize();

            while (br.BaseStream.Position != streamEnd) // will need to check this for accuracy as it has been unreliable in some cases in the past
            {
                Opcode opcode = FileIOHelper.ReadOpcode(br);
                br.BaseStream.Position -= OpcodeInfo.OPCODE_SIZE; // set the position back by 2, the size of the opcodes, as the instruction parsers will expect to given a position starting with an opcode

                if (opcode == Opcode.INVALID)
                {
                    string errorMessage = string.Format("There was an unexpected opcode when reading file {0} at position {1} after reading {2} instructions", fileName, br.BaseStream.Position.ToString("X"), instructions.Count.ToString());
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                IInstruction newInstruction;
                Type instructionParserType = OpcodeInfo.GetInstructionParserType(opcode);
                
                if (instructionParserType == typeof(InstructionBox))
                {
                    newInstruction = MakeInstructionBox(opcode);
                    newInstruction.Read(br);
                }
                else
                {
                    newInstruction = (IInstruction)Activator.CreateInstance(instructionParserType);
                    newInstruction.Read(br);
                }

                instructions.Add(newInstruction);

            }

            //string finishMessage = string.Format("Parsed file {0} with {1} instructions", fileName, instructions.Count.ToString());
            //MessageBox.Show(finishMessage);

            workingFile.instructions = instructions;
            return workingFile;
            
        }

        public void WriteFile(KCFile file, string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            file.header.Write(bw);

            foreach (var instruction in file.instructions)
            {
                instruction.Write(bw);
            }

            file.footer.Write(bw);

            bw.Close();
            fs.Close();
        }

        private IHeader ReadHeader(BinaryReader br)
        {
            St_Header header = new St_Header();
            if (!header.Read(br))
            {
                string errorMessage = "Failed to read header of file {0}. Corrupt or invalid header?";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            return header;
        }

        /// <summary>
        /// Reads the footer of a file
        /// </summary>
        /// <param name="br">The file to be read</param>
        /// <returns>A Box containing the file footer</returns>
        /// <remarks>
        /// Works backwards from the end of the file to calcuate the footer then returns the stream's position back to the start of the stream. Does not preserve the curren position of any streams passed to it.
        /// </remarks>
        private DataBox ReadFooter(BinaryReader br)
        {
            br.BaseStream.Seek(-1, SeekOrigin.End);

            int footerSize = 0;
            DataBox footer;

            if (br.ReadByte() != 0x88)
            {
                br.BaseStream.Seek(0, SeekOrigin.Begin); // is this control flow convoluted?
                footer = new DataBox(0);
                footer.Read(br);
                return footer;
            }

            br.BaseStream.Seek(-1, SeekOrigin.End);

            while (br.ReadByte() == 0x88)
            {
                br.BaseStream.Seek(-2, SeekOrigin.Current);
                footerSize += 1;
            }

            footer = new DataBox(footerSize);
            footer.Read(br);

            br.BaseStream.Seek(0, SeekOrigin.Begin);

            return footer;
        }

    }
}
