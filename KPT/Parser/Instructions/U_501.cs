﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using KPT.Parser.Spreadsheet_Interface;
using KPT.Parser.Elements;

namespace KPT.Parser.Instructions
{
    class U_501 : IInstruction, IHasName, IHasStrings
    {

        Opcode opcode;
        SpriteInfo firstSprite;
        SpriteInfo secondSprite;
        SpriteInfo thirdSprite;
        int voiceClip;
        int unknown;
        string name;
        string dialogue;

        public bool Read(BinaryReader br)
        {
            opcode = FileIOHelper.ReadOpcode(br);
            firstSprite = new SpriteInfo();
            firstSprite.Read(br);
            secondSprite = new SpriteInfo();
            secondSprite.Read(br);
            thirdSprite = new SpriteInfo();
            thirdSprite.Read(br);
            voiceClip = br.ReadInt32();
            unknown = br.ReadInt32();
            name = FileIOHelper.ReadName(br);
            dialogue = FileIOHelper.ReadDialogueString(br);
            return true;
        }

        public bool Write(BinaryWriter bw)
        {
            bw.Write((short)opcode);
            firstSprite.Write(bw);
            secondSprite.Write(bw);
            thirdSprite.Write(bw);
            bw.Write(voiceClip);
            bw.Write(unknown);
            FileIOHelper.WriteFixedLengthString(bw, name, Constants.NAME_LENGTH);
            FileIOHelper.WriteDialogueString(bw, dialogue);
            return true;
        }

        public string GetName()
        {
            return name;
        }

        public void SetName(string newName)
        {
            name = newName;
        }

        public void AddStrings(StringCollection collection)
        {
            string newID = collection.GenerateNewID();
            collection.AddString(newID, dialogue);
            dialogue = newID;
        }

        public void GetStrings(StringCollection collection)
        {
            dialogue = collection.GetString(dialogue);
        }

        public List<CSVRecord> GetCSVRecords()
        {
            return new List<CSVRecord> { new CSVRecord(name, dialogue) };
        }

    }
}
