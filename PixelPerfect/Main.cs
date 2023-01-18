﻿using System;
using System.Diagnostics;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Security;
using System.Numerics;
using Dalamud.Game.ClientState.Resolvers;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Lumina.Excel.GeneratedSheets;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Condition = Dalamud.Game.ClientState.Conditions.Condition;
using Newtonsoft.Json;
using System.Text;

namespace PixelPerfect
{
    public partial class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private readonly DalamudPluginInterface _pi;
        private readonly CommandManager _cm;
        private readonly ClientState _cs;
        private readonly Framework _fw;
        private readonly GameGui _gui;
        private readonly Condition _condition;
        private readonly Config _configuration;
        private bool _config;
        private bool _editor;
        private bool _firstTime;
        private bool _editorHelp;
        private int dirtyHack = 0;
        private string[] doodleOptions;
        private string[] doodleJobs;
        private uint[] doodleJobsUint;
        private  List<Drawing> doodleBag;
        private float editorScale;
        private int selected;
        private int grabbed = -1;
        private bool _update = false;

        private string exported = "Nothing";

        public PixelPerfect(
            DalamudPluginInterface pluginInterface,
            CommandManager commandManager,
            ClientState clientState,
            Framework framework,
            GameGui gameGui,
            Condition condition
        )
        {
            _pi = pluginInterface;
            _cm = commandManager;
            _cs = clientState;
            _fw = framework;
            _gui = gameGui;
            _condition = condition;

            _configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();

            doodleBag = _configuration.DoodleBag;
            if(doodleBag.Count==0 ) { _firstTime = true; }

            if (_configuration.Version < 4)
            {
                _update = true;
                _configuration.Version = 4;
                foreach(var doodle in doodleBag)
                {
                    if (doodle.Job > 0)
                    {
                        doodle.JobsBool[0] = false;
                        doodle.JobsBool[doodle.Job] = true;
                    }
                }
                SaveConfig();
            }

            doodleOptions = new string[4]{ "Ring","Line","Dot","Cone"};
            doodleJobs = new String[21] { "All", "PLD", "WAR", "DRK", "GNB", "WHM", "SCH", "AST", "SGE", "MNK", "DRG", "NIN", "SAM", "RPR", "BRD", "MCH", "DNC", "BLM", "SMN",  "RDM","BLU" };
            doodleJobsUint = new uint[21] { 0, 19, 21, 32, 37, 24, 28, 33, 40, 20, 22, 30, 34, 39, 23, 31, 38, 25, 27, 35, 36 };
            

            editorScale = 4f;
            selected = -1;

            pluginInterface.UiBuilder.Draw += DrawConfig;
            pluginInterface.UiBuilder.Draw += DrawDoodles;
            pluginInterface.UiBuilder.Draw += DrawEditor;
            pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow;
            commandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect config."
            }) ;
        }

        private void ConfigWindow()
        {
            _config = true;
        }


        public void Dispose()
        {
            _pi.UiBuilder.Draw -= DrawConfig;
            _pi.UiBuilder.Draw -= DrawDoodles;
            _pi.UiBuilder.Draw -= DrawEditor;
            _pi.UiBuilder.OpenConfigUi -= ConfigWindow;
            _cm.RemoveHandler("/pp");
        }

        private void Command(string command, string arguments)
        {
             _config = !_config;
            SaveConfig();
        }

        private bool CheckJob(uint jobUint, bool[] jobList)
        {
            var check = false;
            if (jobList[0])
            {
                check = true;
            }
            var loop = 0;
            foreach(var job in jobList )
            {
                if ( job )
                {
                    if (doodleJobsUint[loop] == jobUint)
                    {
                        check = true;
                    }
                }
                loop++;
            }

            return check;
        }
        
          public T ImportSave<T>(string base64Text)
          {
              byte[] bytes = Convert.FromBase64String(base64Text);

              string json = Encoding.Default.GetString(bytes);

              return JsonConvert.DeserializeObject<T>(json);
          }
        
          public static string ExportSave(object obj)
          {
              string json = JsonConvert.SerializeObject(obj);

              byte[] bytes = Encoding.Default.GetBytes(json);

              return Convert.ToBase64String(bytes);
          }
          

        private void SaveConfig()
        {
            _configuration.DoodleBag = doodleBag;


            _pi.SavePluginConfig(_configuration);
        }

        private void DrawRingWorld(Dalamud.Game.ClientState.Objects.Types.Character actor, float radius, int numSegments,int segStart, int segEnd, float thicc, uint colour, bool offset,Vector4 off, bool North)
        {
            //Alter -degree- start somehow

            var xOff = 0f;
            var yOff = 0f;
            if (offset)
            {
                xOff =off.X;
                yOff = off.Y;
            }
            var seg = numSegments / 2;
            var sin = Math.Sin(-_cs.LocalPlayer.Rotation + Math.PI);
            var cos = Math.Cos(-_cs.LocalPlayer.Rotation + Math.PI);

            for (var i = 0; i <= numSegments; i++)
            {
                if (i >= segStart && i <= segEnd)
                
                    var xOld = actor.Position.X + xOff + (radius * (float)Math.Sin((Math.PI / seg) * i));
                    var zOld = actor.Position.Z + yOff + (radius * (float)Math.Cos((Math.PI / seg) * i));

                    var xr1 = cos * (off.W) - sin * (off.X) + actor.Position.X;
                    var yr1 = sin * (off.W) + cos * (off.X) + actor.Position.Z;
                    if (North)
                    {
                        _gui.WorldToScreen(new Vector3(xOld, actor.Position.Y, zOld), out Vector2 pos);
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
                    }
                    else
                    {
                        _gui.WorldToScreen(new Vector3((float)xr1, actor.Position.Y, (float)yr1), out Vector2 pos);
                        ImGui.GetWindowDrawList().PathLineTo(new Vector2(pos.X, pos.Y));
                    }
                    
                    
                }
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        private void DrawArcWorld(Dalamud.Game.ClientState.Objects.Types.Character actor, float radius, int numSegments, int segStart, int segEnd, float thicc, uint colour, bool offset, Vector4 off, bool north)
        {
            //Alter -degree- start somehow

            var xOff = 0f;
            var yOff = 0f;
            if (offset)
            {
                xOff = off.X;
                yOff = off.Y;
            }
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                if(i == segStart | i == segEnd)
                {
                    _gui.WorldToScreen(new Num.Vector3( actor.Position.X ,actor.Position.Y, actor.Position.Z),out Num.Vector2 pos);
                    ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
                }
                if (i >= segStart && i < segEnd)
                {
                    _gui.WorldToScreen(new Num.Vector3(
                        actor.Position.X + xOff + (radius * (float)Math.Sin(((Math.PI) / seg) * i)),
                        actor.Position.Y,
                        actor.Position.Z + yOff + (radius * (float)Math.Cos(((Math.PI) / seg) * i))
                        ),
                        out Num.Vector2 pos);
                    ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
                }
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        private void DrawRingEditor(float dX, float dY, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                var dX2 = dX + (radius * (float)Math.Sin((Math.PI / seg) * i));
                var dY2 = dY + (radius * (float)Math.Cos((Math.PI / seg) * i));

                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(dX2, dY2));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.None, thicc);
        }

        public static JobIds IdToJob(uint job) => job < 19 ? JobIds.OTHER : (JobIds)job;
    }

    public class Drawing
    {
        public String Name { get; set; } = "Doodle";
        public bool Enabled { get; set; } = true;
        public int Job { get; set; } = 0;
        public bool[] JobsBool { get; set; } = new bool[21] { true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        public int Type { get; set; } = 0;
        public Vector4 Colour { get; set; } = new Vector4(1f, 1f, 1f, 1f);
        public bool North { get; set; } = true;
        public float Thickness { get; set; } = 10f;
        public int Segments { get; set; } = 100;
        public float Radius { get; set; } = 2f;
         public Vector4 Vector { get; set; } = new Vector4( -2f, 0f, 0f, 0f);
        public bool Filled { get; set; } =  true;
        public bool Combat { get; set; } = false;
        public bool Instance { get; set; } = false;
        public bool Offset { get; set; } = false;
        public int SegStart { get; set; } = 0;
        public int SegEnd { get; set; } = 100;
    }


    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 4;
        public List<Drawing> DoodleBag { get; set; } = new List<Drawing>();
    }

    public enum JobIds : uint
    {
        OTHER = 0,
        GNB = 37,
        AST = 33,
        PLD = 19,
        WAR = 21,
        DRK = 32,
        SCH = 28,
        WHM = 24,
        BRD = 23,
        DRG = 22,
        SMN = 27,
        SAM = 34,
        BLM = 25,
        RDM = 35,
        MCH = 31,
        DNC = 38,
        NIN = 30,
        MNK = 20,
        BLU = 36,
        RPR = 39,
        SGE = 40
    }
}