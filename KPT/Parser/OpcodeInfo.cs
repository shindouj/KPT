﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPT.Parser.Instruction_Parsers;
using System.Windows.Forms;

namespace KPT.Parser
{  

    enum Opcode
    {
        INVALID = -1,
        EOF = -2,
        U_191 = 0x191,
        U_303 = 0x303,
        U_501 = 0x501,
        U_504 = 0x504,

        U_214 = 0x214,
        U_210 = 0x210,
        U_404 = 0x404,
        U_402 = 0x402,
    }

    static class OpcodeInfo
    {

        public static int OPCODE_SIZE = 2;

        /// <summary>
        /// A dictionary of instructions and their sizes. Size is defined as the combined size of the opcode and any arguments it takes. Mainly used for Boxes.
        /// </summary>
        static Dictionary<Opcode, int> instructionSize = new Dictionary<Opcode, int>
        {
            { Opcode.U_191, 6 },
            { Opcode.U_303, 2 },
            { Opcode.U_214, 4 },
            { Opcode.U_210, 4 },
            { Opcode.U_404, 7 },
            { Opcode.U_402, 2 },
        };

        static Dictionary<Opcode, Type> instructionParserMap = new Dictionary<Opcode, Type>
        {
            { Opcode.U_191, typeof(Box) },

            { Opcode.U_210, typeof(Box) },
            { Opcode.U_214, typeof(Box) },

            { Opcode.U_303, typeof(Box) },         
            
            { Opcode.U_402, typeof(Box) },
            { Opcode.U_404, typeof(Box) },

            { Opcode.U_501, typeof(U_501) },
            { Opcode.U_504, typeof(U_504) },

        };


        public static int GetInstructionSize(Opcode opcode)
        {
            int size;
            bool success;

            success = instructionSize.TryGetValue(opcode, out size);

            if (!success)
            {
                string errorMessage = string.Format("No instruction size specified for opcode {0}.", opcode.ToString());
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return size;
        }

        public static Type GetInstructionParserType(Opcode opcode)
        {

            Type type;
            bool success;

            success = instructionParserMap.TryGetValue(opcode, out type);

            if (!success)
            {
                string errorMessage = string.Format("No instruction parser type specified for opcode {0}.", opcode.ToString());
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            return type;
        }

    }

}