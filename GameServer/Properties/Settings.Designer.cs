﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GameServer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.1.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8701")]
        public ushort GSPort {
            get {
                return ((ushort)(this["GSPort"]));
            }
            set {
                this["GSPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6678")]
        public ushort TSPort {
            get {
                return ((ushort)(this["TSPort"]));
            }
            set {
                this["TSPort"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public ushort PacketLimit {
            get {
                return ((ushort)(this["PacketLimit"]));
            }
            set {
                this["PacketLimit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public ushort AbnormalBlockTime {
            get {
                return ((ushort)(this["AbnormalBlockTime"]));
            }
            set {
                this["AbnormalBlockTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public ushort DisconnectTime {
            get {
                return ((ushort)(this["DisconnectTime"]));
            }
            set {
                this["DisconnectTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("40")]
        public byte MaxLevel {
            get {
                return ((byte)(this["MaxLevel"]));
            }
            set {
                this["MaxLevel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public decimal EquipRepairDto {
            get {
                return ((decimal)(this["EquipRepairDto"]));
            }
            set {
                this["EquipRepairDto"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public decimal ExtraDropRate {
            get {
                return ((decimal)(this["ExtraDropRate"]));
            }
            set {
                this["ExtraDropRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public decimal ExpRate {
            get {
                return ((decimal)(this["ExpRate"]));
            }
            set {
                this["ExpRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public byte LessExpGrade {
            get {
                return ((byte)(this["LessExpGrade"]));
            }
            set {
                this["LessExpGrade"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0.1")]
        public decimal LessExpGradeRate {
            get {
                return ((decimal)(this["LessExpGradeRate"]));
            }
            set {
                this["LessExpGradeRate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("120")]
        public ushort TemptationTime {
            get {
                return ((ushort)(this["TemptationTime"]));
            }
            set {
                this["TemptationTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("5")]
        public byte ItemCleaningTime {
            get {
                return ((byte)(this["ItemCleaningTime"]));
            }
            set {
                this["ItemCleaningTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public byte ItemOwnershipTime {
            get {
                return ((byte)(this["ItemOwnershipTime"]));
            }
            set {
                this["ItemOwnershipTime"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\Database")]
        public string GameData目录 {
            get {
                return ((string)(this["GameData目录"]));
            }
            set {
                this["GameData目录"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\Backup")]
        public string GameDataDirectory {
            get {
                return ((string)(this["GameDataDirectory"]));
            }
            set {
                this["GameDataDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public byte NoobLevel {
            get {
                return ((byte)(this["NoobLevel"]));
            }
            set {
                this["NoobLevel"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SoftwareRegistrationCode {
            get {
                return ((string)(this["SoftwareRegistrationCode"]));
            }
            set {
                this["SoftwareRegistrationCode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1\t1\tWelcome to the world of Legends Eternal, Legends Eternal is more exciting wit" +
            "h you! ")]
        public string SystemAnnounceText {
            get {
                return ((string)(this["SystemAnnounceText"]));
            }
            set {
                this["SystemAnnounceText"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool SendPacketsAsync {
            get {
                return ((bool)(this["SendPacketsAsync"]));
            }
            set {
                this["SendPacketsAsync"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool DebugPackets {
            get {
                return ((bool)(this["DebugPackets"]));
            }
            set {
                this["DebugPackets"] = value;
            }
        }
    }
}