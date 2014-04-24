/* ExtraServerFuncs.cs
  
 * Copyright 2014 by Schwitz Markus ( MarkusSR1984 ) schwitz@sossau.com
 *
 * thanks a lot to a few Plugin Develooper for some help in Forum and with ther Plugins where i could
 * read or copy some code to understand how ProCon Plugins working. 
 * 
 * ExtraServerFuncs is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation, either version 3 of the License,
 * or (at your option) any later version. ExtraServerFuncs is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. You should have received a copy of the
 * GNU General Public License along with ExtraServerFuncs. If not, see http://www.gnu.org/licenses/.  
  
 
*/

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using System.Web;
using System.Data;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Reflection;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;


namespace PRoConEvents
{

//Aliases
using EventType = PRoCon.Core.Events.EventType;
using CapturableEvent = PRoCon.Core.Events.CapturableEvents;



    public class ExtraServerFuncs : PRoConPluginAPI, IPRoConPluginInterface
{


/* Inherited:
    this.PunkbusterPlayerInfoList = new Dictionary<string, CPunkbusterInfo>();
    this.FrostbitePlayerInfoList = new Dictionary<string, CPlayerInfo>();
*/
// VARS TO TRY SOMETHING
    private bool isInitValidWeaponList = false;
    private bool isInitWeaponDictionarys = false;
    private bool isInitMapList = false;
    private bool isInstalledUMM = false;
    private enumBoolYesNo useUmm = enumBoolYesNo.Yes;
    List<string> ValidWeaponList;
    List<string> GameModeList;
    List<string> MapNameList;
    List<TeamScore> currentTeamScores = new List<TeamScore>();
    List<string> tmpBanList = new List<string>();
    List<string> HandgunList = new List<string>();
    List<string> BoltActionList = new List<string>();
    List<string> AutoSniperList = new List<string>();
    List<string> ShotgunList = new List<string>();
    List<string> MeleeList = new List<string>();

    Dictionary<string, List<string>> ModeProhibitedWeapons = new Dictionary<string,List<string>>();
    Dictionary<string, List<string>> MapProhibitedWeapons = new Dictionary<string, List<string>>();

  
// Update Check Variables
private    Match match;
private    Match onlineversion;
private    Match localversion;
private    int localV = 0;
private    int onlineV = 0;
private    bool CheckedOnUpdates = false;
// Update Check Variables



// Extra Task Manager Variables
    private Hashtable PluginInfo = new Hashtable();
    private bool isRegisteredInTaskPlaner = false;
    private List<string> Commands = new List<string>();
    private List<string> Variables = new List<string>();
// Extra Task Manager Variables


// Threads
Thread delayed_message;
Thread countdown_message;
// Classes

TextDatei files;
PlayerDB players;
GitHubClient client = new GitHubClient(); 
//PluginDictionary PRoConPlugins;
// GENERAL VARS  

private bool firstload_sleep = true;
public string game_version = "";
private volatile bool readconfig = false;
public volatile bool plugin_enabled = false;
private volatile bool plugin_loaded = false;
private volatile bool serverInfoloaded = false;
private volatile bool stop_init = false;
private bool taskPlanerUpdateNeeded = false;
private enumBoolYesNo advanced_mode = enumBoolYesNo.No;
private enumBoolYesNo expert_mode = enumBoolYesNo.No;
private enumBoolYesNo pm_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo fm_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo kom_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo pom_isEnabled = enumBoolYesNo.No;

private enumBoolYesNo sm_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo bam_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo dmr_isEnabled = enumBoolYesNo.No;


private enumBoolYesNo mWhitelist_isEnabled  = enumBoolYesNo.No;
private enumBoolYesNo showweaponcode = enumBoolYesNo.No;
private enumBoolYesNo agresive_startup = enumBoolYesNo.No;

private enumBoolYesNo rules_enable = enumBoolYesNo.Yes;
private string rules_method = "both";
private enumBoolYesNo rules_firstjoin = enumBoolYesNo.Yes;
private int rules_time = 3;
private List<string> spawnedPlayer = new List<string>();

private List<string> m_ClanWhitelist;
private List<string> m_PlayerWhitelist;
private volatile string startup_mode_def;
private volatile string startup_mode = "none";
private volatile string tmp_mapList;
private volatile string SwitchInitiator;
private volatile string lastcmdspeaker;
private string currentMapFileName = "";
private string currentGamemode = "";
private int currentRound = 0;
private int totalRounds = 0;
private string friendlyenummaplist;
private int playerCount;
private int maxPlayerCount;
private int ServerInfoCounter = 0;
private string isProhibitedWeapon_Result = "";
private string currServername = "";
private string currServerMessage = "";
private string currServerDescription = "";
private int ServerUptime = -1;
private bool ServerUptimePluginHasReInit = false;

private enumBoolYesNo g_prohibitedWeapons_enable = enumBoolYesNo.No;
private List<string> g_prohibitedWeapons;

private enumBoolYesNo map_prohibitedWeapons_enable = enumBoolYesNo.No;
private enumBoolYesNo Auto_Whitelist_Admins = enumBoolYesNo.No;
private enumBoolYesNo Prevent_Admins_Warn = enumBoolYesNo.No;
private enumBoolYesNo Prevent_WlistPlayers_Warn = enumBoolYesNo.No;


private int g_ActionTbanTime = 60;
private string g_PlayerAction = "kick";


private int mpw_ActionTbanTime = 60;
private string mpw_PlayerAction = "kick";
private int mpw_max_Warns = 2;


private int adminMsgCount;
private BattlelogClient blClient; // BL Client try

private PlayerInfo tmpvar1; // ONLY FOR TEST
private Dictionary<string, PlayerInfo> PlayerDB = new Dictionary<string, PlayerInfo>();
private Dictionary<string, string> MapFileNames;
private Dictionary<string, string> GameModeNames;
private Dictionary<string, string> tmpPluginVariables;


static LocalDataStoreSlot VModeSlot = null;
private List<MaplistEntry> m_listCurrMapList = new List<MaplistEntry>();
private volatile string serverMode = "normal";
private volatile string next_serverMode = "normal";
private volatile CPrivileges currentPrivileges;
private volatile string thermsofuse = "NO";
private volatile bool fIsEnabled;
private volatile int fDebugLevel = 2;
private volatile enumBoolYesNo autoconfig = enumBoolYesNo.No;
private volatile string tmp_autoconfig;
private volatile string lastKiller = "none";  // %Killer%
private volatile string lastWeapon = "none"; // %Weapon%
private volatile string lastVictim = "none"; // %Victim%
private volatile string lastUsedWeapon = "none"; // Weapon


private int countdown_time = 60;


    // Commands
private volatile string nm_commandEnable = "normal";     // NORMAL MODE Command
private volatile string pm_commandEnable = "private";    // PRIVATE MODE Command
private volatile string fm_commandEnable = "flagrun";
private volatile string kom_commandEnable = "knife";
private volatile string pom_commandEnable = "pistol";

private volatile string sgm_commandEnable = "shotgun";
private volatile string bam_commandEnable = "boltaction";
private volatile string dmr_commandEnable = "autosniper";
        
private volatile string switchnow_cmd = "switchnow";     // SWITCHNOW Command
private volatile string rules_command = "rules";         // !rules Command
private volatile string cmd_KickAll = "kickall";         // Command to kick all players

private volatile string cmd_cancel = "cancel";

// Messages - save in Variables to make it editable from user later
/* REPLACEMENTS
%initiator%         Name of the admin who defined a Servermode change
%cmd_switchnow%     switchnow command without !/@//
%cmdspeaker%        Name of the admin who write the last command
%currServermode%    Current Server Mode    
%nextServermode%    Next Server Mode
%Killer%            Last Killer
%Weapon%            Last used Weapon
%Victim%            Last Victim
%kickban%			Is Action Kick or Ban
 */

private volatile string msg_pmKick =            "Sorry, Server is in PRIVATE MODE. Please come back later";
private volatile string player_message =        "INFO Server Switch to %nextServermode% on next Round";
private volatile string admin_message =         "%initiator%, Type !%cmd_switchnow% to end the current round and switch Servermode instantly";
private volatile string msg_switchnow =         "Switch to %nextServermode% NOW!";
private volatile string msg_notInitiator =      "You can not use this command because you are not the Initiator of this Server Mode Change";
private volatile string msg_switchnotdefined =  "You can not use this command because there is not defined a mode to switch to!";
private volatile string msg_normalmode =        "NORMAL MODE";
private volatile string msg_privatemode =       "PRIVATE MODE";

// MSG NOT IN SETTINGS
  
private string msg_flagrunmode =        "FLAGRUN MODE";
private string msg_knifemode =          "KNIFE ONLY MODE";
private string msg_pistolmode =         "PISTOL ONLY MODE";

private string msg_shotgunmode =        "SHOTGUN ONLY MODE";
private string msg_boltaction =         "BOLTACTION ONLY MODE";
private string msg_autosniper =         "DMR(AUTOSNIPER) ONLY MODE";

private string msg_warnBanner =         "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";

private string msg_prohibitedWeapon =   "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_prohibitedWeaponLastWarn = "NEXT TIME %kickban%";
private string msg_prohibitedWeaponKick = "KICKED %Killer% for using %Weapon%";
private string msg_prohibitedWeaponKickPlayer = "%kickban%ED you for using %Weapon%";
private string msg_countdown =           "SERVER SWITCH TO %nextServermode% IN";

private string msg_FlagrunWarn =        "%Killer%, DO NOT KILL AGAIN!";
private string msg_FlagrunLastWarn =    "NEXT TIME %kickban%!!!";
private string msg_FlagrunKick = "kicked %Killer% for Kill";
private string msg_fmKick =             "kicked you for kills on a Flagrun Server";

private string msg_KnifeKick = "KICKED %Killer% for using %Weapon%";
private string msg_komKick = "kicked you for kills with %Weapon% on KNIFE ONLY MODE";
private string msg_KnifeWarn =          "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_KnifeLastWarn =      "NEXT TIME %kickban%!!!";

private string msg_pomKick = "kicked you for kills with %Weapon% on PISTOL ONLY MODE";
private string msg_PistolKick = "KICKED %Killer% for using %Weapon%";
private string msg_PistolWarn =         "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_PistolLastWarn =     "NEXT TIME %kickban%!!!";

private string msg_smKick = "kicked you for kills with %Weapon% on SHOTGUN ONLY MODE";
private string msg_ShotgunKick = "KICKED %Killer% for using %Weapon%";
private string msg_ShotgunWarn = "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_ShotgunLastWarn = "NEXT TIME %kickban%!!!";

private string msg_baKick = "kicked you for kills with %Weapon% on BOLTACTION ONLY MODE";
private string msg_BoltActionKick = "KICKED %Killer% for using %Weapon%";
private string msg_BoltActionWarn = "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_BoltActionLastWarn = "NEXT TIME %kickban%!!!";

private string msg_dmrKick = "kicked you for kills with %Weapon% on PISTOL ONLY MODE";
private string msg_DmrKick = "KICKED %Killer% for using %Weapon%";
private string msg_DmrWarn = "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_DmrLastWarn = "NEXT TIME %kickban%!!!";
        
        
private string msg_ActionTypeKick =     "KICK";
private string msg_ActionTypeBan =      "BAN";



  
 

    
// NORMAL MODE VARS
List<string> nmPRoConConfig = new List<string>();

private string nm_Servername = "Your Server Name";
private string nm_Serverdescription = "Your Server Description";
private string nm_ServerMessage = "Your Server Message";
private enumBoolYesNo nm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int nm_VehicleSpawnCount = 100;
private int nm_PlayerSpawnCount = 100;
private List<string> nm_Rules;
private List<string> nm_MapList;
private int g_max_Warns = 2;


// PRIVATE MODE VARS
List<string> pmPRoConConfig = new List<string>();
private List<string>  pm_ClanWhitelist;
private List<string>  pm_PlayerWhitelist;
private string  pm_Servername = "Servername - PRIVATE MODE";
private string  pm_Serverdescription = "Server is in PRIVATE MODE! Only clanmember and friends can join at the moment";
private string  pm_ServerMessage = "Your Server Message";
private enumBoolYesNo pm_VehicleSpawnAllowed  = enumBoolYesNo.Yes;
private int     pm_VehicleSpawnCount = 100;
private int     pm_PlayerSpawnCount = 100;
private List<string> pm_Rules;
private List<string> pm_MapList;
private int pm_max_Warns = 2;
private enumBoolYesNo autoKickAll = enumBoolYesNo.No;


// FLAGRUN MODE VARS
List<string> fmPRoConConfig = new List<string>();
private List<string> fm_ClanWhitelist;
private List<string> fm_PlayerWhitelist;
private string fm_Servername = "Servername - FLAGFUN";
private string fm_Serverdescription = "Server is in Flagrun mode! DO NOT KILL! KILL = KICK or BAN! Play fair and have fun.";
private string fm_ServerMessage = "Your Server Message";
private enumBoolYesNo fm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int fm_VehicleSpawnCount = 100;
private int fm_PlayerSpawnCount = 100;
private List<string> fm_Rules;
private List<string> fm_MapList;
private int fm_max_Warns = 2;
private string fm_PlayerAction = "pb_tban";
private int fm_ActionTbanTime = 60;


// KNIFE ONLY VARS
List<string> komPRoConConfig = new List<string>();
private List<string> kom_ClanWhitelist;
private List<string> kom_PlayerWhitelist;
private string kom_Servername = "oxx[|===> KNIFE ONLY <===|]xxo  Servername";
private string kom_Serverdescription = "Server is in KNIFE ONLY MODE!! Do not use any other weapon! Play fair and have fun.";
private string kom_ServerMessage = "Your Server Message";
private enumBoolYesNo kom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int kom_VehicleSpawnCount = 100;
private int kom_PlayerSpawnCount = 100;
private List<string> kom_Rules;
private List<string> kom_MapList;
private int kom_max_Warns = 2;
private string kom_PlayerAction = "pb_tban";
private int kom_ActionTbanTime = 60;




// PISTOL ONLY VARS
List<string> pomPRoConConfig = new List<string>();
private List<string> pom_ClanWhitelist;
private List<string> pom_PlayerWhitelist;
private string pom_Servername = "Servername - PISTOL ONLY";
private string pom_Serverdescription = "Server is in PISTOL ONLY MODE!! Do not use any other weapon! Play fair and have fun.";
private string pom_ServerMessage = "Your Server Message";
private enumBoolYesNo pom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int pom_VehicleSpawnCount = 100;
private int pom_PlayerSpawnCount = 100;
private List<string> pom_Rules;
private List<string> pom_MapList;
private int pom_max_Warns = 2;
private string pom_PlayerAction = "pb_tban";
private int pom_ActionTbanTime = 60;

// SHOTGUN ONLY VARS
List<string> smPRoConConfig = new List<string>();
private List<string> sm_ClanWhitelist;
private List<string> sm_PlayerWhitelist;
private string sm_Servername = "Servername - SHOTGUN ONLY";
private string sm_Serverdescription = "Server is in SHOTGUN ONLY MODE!! Do not use any other weapon! Play fair and have fun.";
private string sm_ServerMessage = "Your Server Message";
private enumBoolYesNo sm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int sm_VehicleSpawnCount = 100;
private int sm_PlayerSpawnCount = 100;
private List<string> sm_Rules;
private List<string> sm_MapList;
private int sm_max_Warns = 2;
private string sm_PlayerAction = "pb_tban";
private int sm_ActionTbanTime = 60;

// BOLTACTION ONLY VARS
List<string> bamPRoConConfig = new List<string>();
private List<string> bam_ClanWhitelist;
private List<string> bam_PlayerWhitelist;
private string bam_Servername = "Servername - BOLTACTION ONLY";
private string bam_Serverdescription = "Server is in BOLTACTION ONLY MODE!! Do not use any other weapon! Play fair and have fun.";
private string bam_ServerMessage = "Your Server Message";
private enumBoolYesNo bam_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int bam_VehicleSpawnCount = 100;
private int bam_PlayerSpawnCount = 100;
private List<string> bam_Rules;
private List<string> bam_MapList;
private int bam_max_Warns = 2;
private string bam_PlayerAction = "pb_tban";
private int bam_ActionTbanTime = 60;

// AUTOSNIPER ONLY VARS
List<string> dmrPRoConConfig = new List<string>();
private List<string> dmr_ClanWhitelist;
private List<string> dmr_PlayerWhitelist;
private string dmr_Servername = "Servername - AUTOSNIPER ONLY";
private string dmr_Serverdescription = "Server is in AUTOSNIPER ONLY MODE!! Do not use any other weapon! Play fair and have fun.";
private string dmr_ServerMessage = "Your Server Message";
private enumBoolYesNo dmr_VehicleSpawnAllowed = enumBoolYesNo.Yes;
private int dmr_VehicleSpawnCount = 100;
private int dmr_PlayerSpawnCount = 100;
private List<string> dmr_Rules;
private List<string> dmr_MapList;
private int dmr_max_Warns = 2;
private string dmr_PlayerAction = "pb_tban";
private int dmr_ActionTbanTime = 60;











// New Weapon Dictionarys for Pistol, Shotgun, DMR, Sniper ( 0.0.3.0 )
Dictionary<string, enumBoolYesNo> Allow_Handguns = new Dictionary<string, enumBoolYesNo>();
Dictionary<string, enumBoolYesNo> Allow_Shotguns = new Dictionary<string, enumBoolYesNo>();
Dictionary<string, enumBoolYesNo> Allow_Autosniper = new Dictionary<string, enumBoolYesNo>();
Dictionary<string, enumBoolYesNo> Allow_Boltaction = new Dictionary<string, enumBoolYesNo>();

        
//// OLD HANDGUN LISTS        
//private enumBoolYesNo pom_allowPistol_M9 = enumBoolYesNo.Yes;               //M9
//private enumBoolYesNo pom_allowPistol_QSZ92 = enumBoolYesNo.Yes;            //QSZ-92
//private enumBoolYesNo pom_allowPistol_MP443 = enumBoolYesNo.Yes;            //MP-443
//private enumBoolYesNo pom_allowPistol_Shorty = enumBoolYesNo.No;            //SHORTY 12G
//private enumBoolYesNo pom_allowPistol_Glock18 = enumBoolYesNo.Yes;          //G18
//private enumBoolYesNo pom_allowPistol_FN57 = enumBoolYesNo.Yes;             //FN57
//private enumBoolYesNo pom_allowPistol_M1911 = enumBoolYesNo.Yes;            //M1911
//private enumBoolYesNo pom_allowPistol_93R = enumBoolYesNo.Yes;              //93R
//private enumBoolYesNo pom_allowPistol_CZ75 = enumBoolYesNo.Yes;             //CZ-75
//private enumBoolYesNo pom_allowPistol_Taurus44 = enumBoolYesNo.Yes;         //.44 MAGNUM
//private enumBoolYesNo pom_allowPistol_HK45C = enumBoolYesNo.Yes;            //COMPACT 45
//private enumBoolYesNo pom_allowPistol_P226 = enumBoolYesNo.Yes;             //P226
//private enumBoolYesNo pom_allowPistol_MP412Rex = enumBoolYesNo.Yes;         //M412 REX
//private enumBoolYesNo pom_allowPistol_SW40 = enumBoolYesNo.Yes;             //SW40
//private enumBoolYesNo pom_allowPistol_Meele = enumBoolYesNo.Yes;            //Knife

private string LogFileName =  @"Plugins\ExtraServerFuncs.log";
    
    
    




public ExtraServerFuncs() {
	fIsEnabled = false;
	fDebugLevel = 2;
    files = new TextDatei();
    //PRoConPlugins = new PluginDictionary();


    nm_Rules = new List<string>();			// NORMAL MODE RULES
    nm_Rules.Add("############## RULES ##############");
    nm_Rules.Add("NO CHEATING / GLITCHING / BUGUSING");

    pm_Rules = new List<string>(); 			// PRIVATE MODE RULES
    pm_Rules.Add("########## PRIVATE MODE ##########");
    pm_Rules.Add("NO RULES ARE ACTIVE");

    kom_Rules = new List<string>(); 			// KNIFE ONLY MODE RULES
    kom_Rules.Add("########### KNIFE ONLY ###########");
    kom_Rules.Add("KNIFE ONLY! DO NOT USE ANY OTHER WEAPON");

    pom_Rules = new List<string>(); 			// PISTOL ONLY MODE RULES
    pom_Rules.Add("########### PISTOL ONLY ###########");
    pom_Rules.Add("PISTOL ONLY! DO NOT USE ANY OTHER WEAPON");
        
    fm_Rules = new List<string>(); 			// FLAGRUN MODE RULES
    fm_Rules.Add("############# FLAGRUN #############");
    fm_Rules.Add("DO NOT KILL - KILL = KICK or BAN");
    fm_Rules.Add("NO FLAGCAMPING");
        
    g_prohibitedWeapons = new List<string>();
    g_prohibitedWeapons.Add("READ PLUGIN DESCRIPTION");
    g_prohibitedWeapons.Add("TO KNOW HOW U GET THE WEAPONCODES EASY");
    
    nm_MapList = new List<string>();			// NORMAL MODE MAPLIST
    nm_MapList.Add("MP_Prison Domination0 2");
    nm_MapList.Add("MP_Naval ConquestSmall0 2");
    nm_MapList.Add("MP_Damage Elimination0 2");
    nm_MapList.Add("MP_Resort Obliteration 2");
    nm_MapList.Add("MP_Prison RushLarge0 2");
    nm_MapList.Add("MP_Journey SquadDeathMatch0 2");
    nm_MapList.Add("MP_TheDish TeamDeathMatch0 2");
    nm_MapList.Add("XP1_001 AirSuperiority0 2");
        
    pm_MapList = new List<string>(); 			// PRIVATE MODE MAPLIST
    pm_MapList.Add("MP_Journey TeamDeathMatch0 2"); // Goldmud
    pm_MapList.Add("MP_Prison TeamDeathMatch0 2");  // Spind
        
    pom_MapList = new List<string>(); 			// PISTOL ONLY MODE MAPLIST
    pom_MapList.Add("MP_Prison TeamDeathMatch0 2");  // Spind

    kom_MapList = new List<string>(); 			// KNIFE ONLY MODE MAPLIST
    kom_MapList.Add("MP_Prison TeamDeathMatch0 2");  // Spind
    
    fm_MapList = new List<string>(); 			// FLAGRUN MODE MAPLIST
    fm_MapList.Add("MP_Flooded ConquestLarge0 2"); // Floodzone
    fm_MapList.Add("MP_Journey ConquestLarge0 2"); // Goldmud

    tmpPluginVariables = new Dictionary<string, string>();
    
    
    
    MapFileNames = new Dictionary<string, string>();  // Map Names an Filenames



	m_ClanWhitelist = new List<string>();		// General Clan Whitelist
	m_PlayerWhitelist = new List<string>();		// General Player Whitelist
	pm_ClanWhitelist = new List<string>();		// PRIVATE MODE Clan Whitelist
	pm_PlayerWhitelist = new List<string>();	// PRIVATE MODE Player Whitelist
    fm_ClanWhitelist = new List<string>();		// FLAGRUN MODE Clan Whitelist
    fm_PlayerWhitelist = new List<string>();	// FLAGRUN MODE Player Whitelist
    kom_ClanWhitelist = new List<string>();		// KNIFE ONLY MODE Clan Whitelist
    kom_PlayerWhitelist = new List<string>();	// KNIFE MODE Player Whitelist
    pom_ClanWhitelist = new List<string>();		// KNIFE ONLY MODE Clan Whitelist
    pom_PlayerWhitelist = new List<string>();	// KNIFE MODE Player Whitelist




}

private void InitMapList()
{
    WritePluginConsole("[InitMapList]", "DEBUG", 6);
    GameModeList = new List<string>();
    MapNameList = new List<string>();
    MapFileNames = new Dictionary<string, string>();
    GameModeNames = new Dictionary<string, string>();
    
    
    
    foreach (CMap map in GetMapDefines())
    {
        //                          Anzeigename              Anzeige Mode        Mapfilename             mapGamemode
        // string formattedMap = map.PublicLevelName + " " + map.GameMode + " " + map.FileName + " " + map.PlayList;
        
        if (!GameModeList.Contains(map.GameMode))
        {
            GameModeList.Add(map.GameMode);
            GameModeNames.Add(map.GameMode, map.PlayList);
        }
        
        if (!MapFileNames.ContainsKey(map.PublicLevelName))
        {
            MapNameList.Add(map.PublicLevelName);
            MapFileNames.Add(map.PublicLevelName, map.FileName);
        }
    


    }


    isInitMapList = true;
}


public bool IsUMM_installed()
{
    if (!isInstalledUMM)
    {

        List<MatchCommand> registered = this.GetRegisteredCommands();
        foreach (MatchCommand command in registered)
        {
            if (command.RegisteredClassname.CompareTo("CUltimateMapManager") == 0)
            {
                WritePluginConsole("^bUltimate Map Manager^n detected", "INFO", 3);
                return true;
            }

        }
    }

    return isInstalledUMM;
    

}



public bool IsExtraTaskPlanerInstalled()
{
    List<MatchCommand> registered = this.GetRegisteredCommands();
    foreach (MatchCommand command in registered)
    {
        if (command.RegisteredClassname.CompareTo("ExtraTaskPlaner") == 0 && command.RegisteredMethodName.CompareTo("PluginInterface") == 0)
        {
            WritePluginConsole("^bExtra Task Planer^n detected", "INFO", 3);
            return true;
        }

    }

    return false;
}

public void ExtraTaskPlaner_Callback(string command)
{

    if (command == "success")
    {
        isRegisteredInTaskPlaner = true;
    }

    if (command == "Define normal mode as next") PreSwitchServerMode("normal");
    if (pm_isEnabled == enumBoolYesNo.Yes && command == "Define private mode as next") PreSwitchServerMode("private");
    if (fm_isEnabled == enumBoolYesNo.Yes && command == "Define flagrun mode as next") PreSwitchServerMode("flagrun");
    if (kom_isEnabled == enumBoolYesNo.Yes && command == "Define knife only mode as next") PreSwitchServerMode("knife");
    if (pom_isEnabled == enumBoolYesNo.Yes && command == "Define pistol only mode as next") PreSwitchServerMode("pistol");


    if (command == "Switch to normal mode")
    {
        PreSwitchServerMode("normal");
        if (playerCount > 1) StartSwitchCountdown();
    }


    if (pm_isEnabled == enumBoolYesNo.Yes && command == "Switch to private mode")
    {
        PreSwitchServerMode("private");
        if (playerCount > 1) StartSwitchCountdown();
    }

    if (fm_isEnabled == enumBoolYesNo.Yes && command == "Switch to flagrun mode")
    {
        PreSwitchServerMode("flagrun");
        if (playerCount > 1) StartSwitchCountdown();
    }

    if (kom_isEnabled == enumBoolYesNo.Yes && command == "Switch to knife only mode")
    {
        PreSwitchServerMode("knife");
        if (playerCount > 1) StartSwitchCountdown();
    }

    if (pom_isEnabled == enumBoolYesNo.Yes && command == "Switch to pistol only mode")
    {
        PreSwitchServerMode("pistol");
        if (playerCount > 1) StartSwitchCountdown();
    }






}


private string GetCurrentClassName()
{
    string tmpClassName;

    tmpClassName = this.GetType().ToString(); // Get Current Classname String
    tmpClassName = tmpClassName.Replace("PRoConEvents.", "");


    return tmpClassName;

}

private int GetWinningTeamID() // Ermittle das Team das gerade besser ist und gebe die Team Nummer zurueck
{
    WritePluginConsole("[GetWinningTeamID]", "DEBUG", 6);

    TeamScore tmpTeam = new TeamScore(0,0);


    foreach (TeamScore team in currentTeamScores)
    {
        WritePluginConsole("[GetWinningTeamID] TeamInfo: TeamID = " + team.TeamID + " Score = " + team.Score + " WinningScore = " + team.WinningScore, "DEBUG", 8);

        if (team.Score < team.WinningScore) // TDM, SQDM , etc...
        {
            if (team.Score > tmpTeam.Score)
            {
                tmpTeam = team;
            }

        }

        if (team.Score > team.WinningScore) // CQ, DOM , etc...
        {
            if (team.Score > tmpTeam.Score)
            {
                tmpTeam = team;
            }

        }

    }

    WritePluginConsole("[GetWinningTeamID] Winning Team is:  TeamID = " + tmpTeam.TeamID + " Score = " + tmpTeam.Score + " WinningScore = " + tmpTeam.WinningScore, "INFO", 6);
    return tmpTeam.TeamID;

}


private void SendTaskPlanerInfo()
{
    taskPlanerUpdateNeeded = false;

    Commands = new List<string>();

    Commands.Add("Define normal mode as next");           // You have to catch this Commands in Method ExtraTaskPlaner_Callback()

    if (pm_isEnabled == enumBoolYesNo.Yes) Commands.Add("Define private mode as next");
    if (fm_isEnabled == enumBoolYesNo.Yes) Commands.Add("Define flagrun mode as next");
    if (kom_isEnabled == enumBoolYesNo.Yes) Commands.Add("Define knife only mode as next");
    if (pom_isEnabled == enumBoolYesNo.Yes) Commands.Add("Define pistol only mode as next");

    Commands.Add("Switch to normal mode");

    if (pm_isEnabled == enumBoolYesNo.Yes)    Commands.Add("Switch to private mode");
    if (fm_isEnabled == enumBoolYesNo.Yes) Commands.Add("Switch to flagrun mode");
    if (kom_isEnabled == enumBoolYesNo.Yes) Commands.Add("Switch to knife only mode");
    if (pom_isEnabled == enumBoolYesNo.Yes) Commands.Add("Switch to pistol only mode");


    //Variables.Add("Sample Variable");        I dont want to give out any Variable to Task Manager


    



    PluginInfo["PluginName"] = GetPluginName();
    PluginInfo["PluginVersion"] = GetPluginVersion();
    PluginInfo["PluginClassname"] = GetCurrentClassName();

    PluginInfo["PluginCommands"] = CPluginVariable.EncodeStringArray(Commands.ToArray());
    PluginInfo["PluginVariables"] = CPluginVariable.EncodeStringArray(Variables.ToArray());

    this.ExecuteCommand("procon.protected.plugins.setVariable", "ExtraTaskPlaner", "RegisterPlugin", JSON.JsonEncode(PluginInfo)); // Send Plugin Infos to Task Planer

}


private void InitValidWeaponList()
{
    
    WeaponDictionary weapons = GetWeaponDefines();  // Create a List of Weapons to Validate the WeaponCodes in prohibited Weapon Lists
    ValidWeaponList = new List<string>();
    foreach (Weapon weapon in weapons)
    {
        ValidWeaponList.Add(FWeaponName(weapon.Name));

        if (weapon.Damage == DamageTypes.Handgun)   // Create List of Pistols
        {
            HandgunList.Add(FWeaponName(weapon.Name));
        }

        if (weapon.Damage == DamageTypes.SniperRifle) // Create List of Bolt Action Rifles
        {
            BoltActionList.Add(FWeaponName(weapon.Name));
        }

        if (weapon.Damage == DamageTypes.DMR)       // Create List of Auto Sniper Rifles
        {
            AutoSniperList.Add(FWeaponName(weapon.Name));
        }

        if (weapon.Damage == DamageTypes.Shotgun)       // Create List of Shotguns
        {
            ShotgunList.Add(FWeaponName(weapon.Name));
        }

        if (weapon.Damage == DamageTypes.Melee)       // Create List of Melee type Weapons ( Melee, Defigbrilator, ???)
        {
            MeleeList.Add(FWeaponName(weapon.Name));
        }

    }
       
    isInitValidWeaponList = true;
    WritePluginConsole("[InitValidWeaponList] Weaponlist Contains " + (ValidWeaponList.Count).ToString() + " Weapons", "DEBUG", 6);
}

private void InitWeaponDictionarys()
{

    foreach (string _tmpWeapon in MeleeList) // Add Melees to each Dictionary
    {
        if (!Allow_Handguns.ContainsKey(_tmpWeapon)) Allow_Handguns.Add(_tmpWeapon, enumBoolYesNo.Yes);
        if (!Allow_Shotguns.ContainsKey(_tmpWeapon)) Allow_Shotguns.Add(_tmpWeapon, enumBoolYesNo.Yes);
        if (!Allow_Autosniper.ContainsKey(_tmpWeapon)) Allow_Autosniper.Add(_tmpWeapon, enumBoolYesNo.Yes);
        if (!Allow_Boltaction.ContainsKey(_tmpWeapon)) Allow_Boltaction.Add(_tmpWeapon, enumBoolYesNo.Yes);
    }
       
    foreach (string _tmpWeapon in HandgunList)  // HANDGUNS
    {
        if (!Allow_Handguns.ContainsKey(_tmpWeapon)) Allow_Handguns.Add(_tmpWeapon, enumBoolYesNo.Yes);

    }

    foreach (string _tmpWeapon in ShotgunList) // Shotguns
    {
        if (!Allow_Shotguns.ContainsKey(_tmpWeapon)) Allow_Shotguns.Add(_tmpWeapon, enumBoolYesNo.Yes);

    }

    foreach (string _tmpWeapon in BoltActionList) // Bolt Action Sniper Rifles
    {
        if (!Allow_Boltaction.ContainsKey(_tmpWeapon)) Allow_Boltaction.Add(_tmpWeapon, enumBoolYesNo.Yes);

    }

    foreach (string _tmpWeapon in AutoSniperList) // DMR, Autosniper 
    {
        if (!Allow_Autosniper.ContainsKey(_tmpWeapon)) Allow_Autosniper.Add(_tmpWeapon, enumBoolYesNo.Yes);

    }

    isInitWeaponDictionarys = true;

}




private void PluginCommand(string cmd) // Set Speaker to Server if no Speaker is send. e.G. command from ProConPlugin Config
{
PluginCommand("Server", cmd);
}

private void PluginCommand(string cmdspeaker, string cmd) // Routine zur Bereitstellung von Plugin und Chat Commands in Procon
{
    
    if (plugin_enabled || cmd == "try")
    {

        cmd = cmd.Replace(" ", ""); // Leerzeichen entfernen
        lastcmdspeaker = cmdspeaker;

 
        if (cmd == cmd_KickAll)
        {
            KickAll();
        }
        

        if (cmd == nm_commandEnable) 
        {
            SwitchInitiator = cmdspeaker;
            PreSwitchServerMode("normal");
            return;
        }


        if (cmd == pm_commandEnable) 
        {
            SwitchInitiator = cmdspeaker;
            PreSwitchServerMode("private");
            return;
        }


        if (cmd == fm_commandEnable)  
        {
            SwitchInitiator = cmdspeaker;
            PreSwitchServerMode("flagrun");
            return;
        }

        if (cmd == kom_commandEnable) 
        {
            SwitchInitiator = cmdspeaker;
            PreSwitchServerMode("knife");
            return;
        }

        if (cmd == pom_commandEnable) 
        {
            SwitchInitiator = cmdspeaker;
            PreSwitchServerMode("pistol");
            return;
        }
        
        
        if (cmd == switchnow_cmd) 
        {
            if (IsSwitchDefined())
            {
                if (SwitchInitiator == cmdspeaker) StartSwitchCountdown();
                if (SwitchInitiator != cmdspeaker) SendPlayerMessage(cmdspeaker, R(msg_notInitiator));
                return;
            }



            if (!IsSwitchDefined()) SendPlayerMessage(cmdspeaker, R(msg_switchnotdefined));
            return;
        }



        if (cmd == "readconfig" && cmdspeaker == "Server")
        {
            WritePluginConsole("readconfig command sucess...", "DEBUG", 10);
            ReadServerConfig();
            return;
        }

        if (cmd == "update")
        {
            UpdateSettingPage();
            return;
        }
        
        if (cmd == "try")
        {
            
            WritePluginConsole("Start test...", "TRY", 0);

            WritePluginConsole("Current ServerUptime is: " + ServerUptime, "TRY", 0);
            WritePluginConsole("Current Winning Team is: Team " + GetWinningTeamID().ToString(), "TRY", 0);

            foreach (string pistol in HandgunList)
            {
                WritePluginConsole("HANDGUN: " + pistol, "DEBUG", 0);

            }

            //foreach (KeyValuePair<string, string> tmpVar in tmpPluginVariables)
            //{
            //    WritePluginConsole(tmpVar.Key + "("+tmpVar.Value +")", "TMPVARIABLE", 0);
            //}
            
            
            // Funktioniert noch nicht
            
            //foreach (string plugin in LoadedClassNames())
            //{
            //    WritePluginConsole("PLUGIN NAME: " + plugin , "TRY", 0);
            //}

            



            //foreach (CMap map in GetMapDefines())
            //{

            //    // Anzeigename , Anzeige Mode, Mapfilename, mapGamemode
            //    string formattedMap = map.PublicLevelName + " " + map.GameMode + " " + map.FileName + " " + map.PlayList;
            //    WritePluginConsole(formattedMap, "MAPLISTENTRY", 0);    


            //}

            

            //foreach (KeyValuePair<string, string> pair in tmpPluginVariables)
            //{
            //    WritePluginConsole("^b" + pair.Key + "^n ( " + pair.Value + " )", "VARIABLE[GETCACHE]", 0);    
                
            //}

           

            //foreach (CMap map in GetMapDefines()) 
            //{
                    
            //        string formattedMap = map.PublicLevelName + " " + map.GameMode + " " + map.FileName + " " + map.PlayList;
            //        WritePluginConsole("[MAPLIST] " +  formattedMap  ,"TRY",0);

            //}
            
            //WritePluginConsole("Overflow the debug.logfile", "TRY", 0);

            //for (int t = 0; t < 5000; t++)
            //{
            //    WritePluginConsole("This is a overflow test, This is line number:  " + t.ToString(), "TEST", 0);
            //}
            


            ////this.ExecuteCommand("procon.protected.tasks.add", "try", "10", "10", "-1", "procon.protected.send", "admin.yell", "Dies ist ein Test", "3" , "all");
            
            //this.ExecuteCommand("procon.protected.plugins.call", "ExtraServerFuncs", "OnTaskTriggered", "TRY");

            
            //this.ExecuteCommand("procon.protected.tasks.add", "try", "30", "10", "-1", "procon.protected.plugins.call", "ExtraServerFuncs", "OnTaskTriggered");
            
            /*
            tmpvar1 = new PlayerInfo();
            List<string> currplayers = new List<string>();

            currplayers = players.ListPlayers();
            
            
            foreach (string player in currplayers)
            {
            tmpvar1 = players.GetPlayerInfo(player);

            WritePluginConsole("tmpvar1.Name = " + tmpvar1.Name, "TRY", 0);
            WritePluginConsole("tmpvar1.Tag = " + tmpvar1.Tag, "TRY", 0);
            WritePluginConsole("tmpvar1.Kills = " + tmpvar1.Kills, "TRY", 0);
            WritePluginConsole("tmpvar1.Death = " + tmpvar1.Death, "TRY", 0);
            WritePluginConsole("tmpvar1.Warns = " + tmpvar1.Warns, "TRY", 0);
            WritePluginConsole("tmpvar1.Suicides = " + tmpvar1.Suicides, "TRY", 0);

            }
            

           
            
            // WritePluginConsole("tmpvar.Name = "+ tmpvar.Name, "TRY", 0);



            //public string Name;
            //public string Tag;
            //public int Kills;
            //public int Death;
            //public int Warns;
        */
            return;
        }



        WritePluginConsole("Unknown Command", "ERROR", 0);
        return;

    }
	
}


public string FWeaponName(String killWeapon)  // Returns a String with the Used Weapon
{
    KillWeaponDetails r = new KillWeaponDetails();
    r.Name = killWeapon;


    if (killWeapon.StartsWith("U_"))
    {
        String[] tParts = killWeapon.Split(new[] { '_' });

        if (tParts.Length == 2)
        { // U_Name
            r.Name = tParts[1];
        }
        else if (tParts.Length == 3)
        { // U_Name_Detail
            r.Name = tParts[1];
            r.Detail = tParts[2];
        }
        else if (tParts.Length == 4)
        { // U_AttachedTo_Name_Detail
            r.Name = tParts[2];
            r.Detail = tParts[3];
            r.AttachedTo = tParts[1];
        }
        else
        {
            WritePluginConsole("Warning: unrecognized weapon code: " + killWeapon,"DEBUG", 8);
        }
    }
    return r.Name;
}





public KillWeaponDetails FriendlyWeaponName(String killWeapon)  // Returns all parts of Killweapon String
{
    KillWeaponDetails r = new KillWeaponDetails();
    r.Name = killWeapon;

   
    if (killWeapon.StartsWith("U_"))
    {
        String[] tParts = killWeapon.Split(new[] { '_' });

        if (tParts.Length == 2)
        { // U_Name
            r.Name = tParts[1];
        }
        else if (tParts.Length == 3)
        { // U_Name_Detail
            r.Name = tParts[1];
            r.Detail = tParts[2];
        }
        else if (tParts.Length == 4)
        { // U_AttachedTo_Name_Detail
            r.Name = tParts[2];
            r.Detail = tParts[3];
            r.AttachedTo = tParts[1];
        }
        else
        {
            WritePluginConsole("Warning: unrecognized weapon code: " + killWeapon,"DEBUG", 8);
        }
    }
    return r;
}

private void ReadServerConfig()
{
    WritePluginConsole("Call ReadServerConfig()", "DEBUG", 10);
    readconfig = true;
    WritePluginConsole(R("Read ServerVars and save them to %currServermode%"), "INFO", 0);
       
    
    // Refrech Server Vars to catch them with the ON Funktions
    this.ServerCommand("vars.serverName");                 
    this.ServerCommand("vars.serverDescription");      
    this.ServerCommand("vars.serverMessage");
    this.ServerCommand("vars.vehicleSpawnAllowed");  
    this.ServerCommand("vars.vehicleSpawnDelay");
    this.ServerCommand("vars.playerRespawnTime");
    this.ServerCommand("mapList.list");
    
    readconfig = false;
   
    WritePluginConsole(R("[READY] Saved the Serverconfig to: %currServermode%! Please refresh our Config Window"), "Info", 0);
}

private void UdateServerConfig()
{
    WritePluginConsole("Call UpdateServerConfig()", "DEBUG", 10);
    
    


    // Refrech Server Vars to catch them with the ON Funktions
    this.ServerCommand("vars.serverName");
    this.ServerCommand("vars.serverDescription");
    this.ServerCommand("vars.serverMessage");
    this.ServerCommand("vars.vehicleSpawnAllowed");
    this.ServerCommand("vars.vehicleSpawnDelay");
    this.ServerCommand("vars.playerRespawnTime");
    this.ServerCommand("mapList.list");
    this.ServerCommand("mapList.list");
    this.ServerCommand("admin.listPlayers", "all");
    this.ServerCommand("serverinfo");
}	






private void ShowRules()
		{
		ShowRules("all");
		}
	
private void ShowRules(string playerName)
		{
			WritePluginConsole("Read Servermode for show rules: " + serverMode, "Info", 6);
			if (serverMode == "normal")  ShowRules(playerName, nm_Rules, false);
            if (serverMode == "private") ShowRules(playerName, pm_Rules, false);
            if (serverMode == "knife") ShowRules(playerName, kom_Rules, false);
            if (serverMode == "pistol") ShowRules(playerName, pom_Rules, false);
            if (serverMode == "flagrun") ShowRules(playerName, fm_Rules, false);
            
		}

private void ShowRules(string playerName , bool firstspawn)
{
    WritePluginConsole("Read Servermode for show rules: " + serverMode, "Info", 6);
    if (serverMode == "normal") ShowRules(playerName, nm_Rules, firstspawn);
    if (serverMode == "private") ShowRules(playerName, pm_Rules, firstspawn);
    if (serverMode == "knife") ShowRules(playerName, kom_Rules, firstspawn);
    if (serverMode == "pistol") ShowRules(playerName, pom_Rules, firstspawn);
    if (serverMode == "flagrun") ShowRules(playerName, fm_Rules, firstspawn);

}


private void ShowRules(string playerName, List<string> RulesList , bool firstspawn)
		{
            int delay = 0;

            if (firstspawn) delay = 7;  // Set waittime for first spawn
    
            foreach (string rule in RulesList)
			{
				WritePluginConsole("Sending Rule to " + playerName + " rule: " + rule, "Info", 6);
                
                if (rules_method == "chat")
                {
                    if (playerName == "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.say", StripModifiers(E(rule)), "all");
                    if (playerName != "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.say", StripModifiers(E(rule)), "player", playerName);
                }
                if (rules_method == "yell")
                {
                    if (playerName == "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.yell", StripModifiers(E(rule)), rules_time.ToString(), "all");
                    if (playerName != "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.yell", StripModifiers(E(rule)), rules_time.ToString(), "player", playerName);
                }
                if (rules_method == "both")
                {


                    if (playerName == "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.say", StripModifiers(E(rule)), "all");
                    if (playerName != "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.say", StripModifiers(E(rule)), "player", playerName);
                    if (playerName == "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.yell", StripModifiers(E(rule)), rules_time.ToString(), "all");
                    if (playerName != "all") this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", delay.ToString(), "1", "1", "procon.protected.send", "admin.yell", StripModifiers(E(rule)), rules_time.ToString(), "player", playerName);
                }
                delay = delay + rules_time;

			}
		}

public static String InGameCommand_Pattern = @"^\s*([@/!\?])\s*";

public bool IsCommand(String text)
        {
            return Regex.Match(text, InGameCommand_Pattern).Success;
        }

public String ExtractCommand(String text)
        {
            return Regex.Replace(text, InGameCommand_Pattern, "").Trim();
        }

public String ExtractCommandPrefix(String text)
        {
            Match match = Regex.Match(text, InGameCommand_Pattern, RegexOptions.IgnoreCase);

            if (match.Success)
                return match.Groups[1].Value;

            return String.Empty;
        }

public void SwitchNow() // Switch to NEW SERVERMODE INSTANTLY         
				{
                    WritePluginConsole("Called SwitchNow(): serverMode= "+serverMode+" next_serverMode = " + next_serverMode , "Warn", 6);
                    if (plugin_enabled)
                    {
                        SendPlayerMessage(SwitchInitiator, R(msg_switchnow));
                        WritePluginConsole(R(msg_switchnow), "Info", 0);
                        SwitchServerMode(next_serverMode);
                    }
				}

public string boolToStringYesNo(bool isTrue)  // Convertes bool "True/False" into String "Yes/No"
    {

        if (isTrue) return "Yes";
        return "No";

    }

public string enumboolToStringTrueFalse(enumBoolYesNo isTrue)  // Convertes bool "True/False" into String "true/false"
    {

        if (isTrue == enumBoolYesNo.Yes) return "true";
        return "false";

    }
	
public bool kickPlayer(string name,string reason ,int delay)
    {
           
            Thread delayed_kick = new Thread(new ThreadStart(delegate()
            {                 
                Thread.Sleep(delay * 1000);
                kickPlayer(name, reason);
            }));
            delayed_kick.Start();
            return true;
    }

public bool kickPlayer(string pName)
	{
		return kickPlayer(pName, "");		
	}
	
public bool kickPlayer(string pName, string reason)
	{
	WritePluginConsole("Kicking Player "+ pName + " for reason " + reason, "Info", 2);
	this.ExecuteCommand("procon.protected.send", "admin.kickPlayer", pName, reason);
	return true;
	}

public bool tbanPlayer(string name, int minutes, string message)
    {
        //message = message + " (tBan " + (minutes).ToString() + " minutes) by [AUTOADMIN]"; // Verursacht Probleme im Private mode
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", name, "seconds", (minutes * 60).ToString(), message);
        this.ExecuteCommand("procon.protected.send", "banList.save");
       // kickPlayer(name, message);
        return true;
    }

public bool delayedTbanPlayer(string name, int minutes, string message ,int delay)
{
   // message += " (tBan " + (minutes).ToString() + " minutes) by [AUTOADMIN]"; // Verursacht Probleme im Private mode
    this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_DelayedBan", delay.ToString(), "1", "1", "procon.protected.send", "banList.add", "name", name, "seconds", (minutes * 60).ToString(), message);
    this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_DelayedBan", (delay + 1).ToString(), "1", "1", "procon.protected.send", "banList.save");
    return true;
}




public bool pbanPlayer(string name, string message)
    {
        message = message + " (Permanent Ban) by [AUTOADMIN]";
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", name,"perm", message);
        this.ExecuteCommand("procon.protected.send", "banList.save");
       // kickPlayer(name, message);
        return true;
    }

public bool pb_tbanPlayer(string name, int minutes, string message)
    {
        message = message + " (tBan " + (minutes).ToString() + " minutes) by [AUTOADMIN]";
        this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_kick", name, (minutes).ToString(), message, "|", "BC2!" }));
		this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_updbanfile" }));
        
		kickPlayer(name, message);
        return true;
    }

public bool pb_pbanPlayer(string name, string message)
    {
        message = message + " (Permanent Ban) by [AUTOADMIN]";
        this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_ban", name, message, "|", "BC2!" }));
		this.ServerCommand("punkBuster.pb_sv_command", String.Join(" ", new string[] { "pb_sv_updbanfile" }));
        
		kickPlayer(name, message);
        return true;
    }
	
public bool KillPlayer(String name, int delay)
    {
            
            Thread delayed_kill = new Thread(new ThreadStart(delegate()
            {
                Thread.Sleep(delay * 1000);
                KillPlayer(name);
            }));

            delayed_kill.Start();

            return true;
    }

public bool KillPlayer(String name)
        {
            this.ServerCommand("admin.killPlayer", name);
            return true;
        }



private void KickAll()
{
    WritePluginConsole("KickAll()", "DEBUG", 10);
    List<string> tmp_Playerlist = players.ListPlayers();

    foreach (string player in tmp_Playerlist)
    {
        if (!IsAdmin(player) && !isInWhitelist(player)) kickPlayer(player);
    }


}
	
private bool IsAdmin(string speaker) 
    {
      bool isAdmin = false;
      currentPrivileges = GetAccountPrivileges(speaker);

      if (currentPrivileges != null)
      {
        if (currentPrivileges.CanLogin)
          isAdmin = true;
		  
			/*
		        .CanKillPlayers
                .CanKickPlayers
                .CanTemporaryBanPlayers
				.CanPermanentlyBanPlayers
                .CanMovePlayers
                .CanUseMapFunctions
				.CanLogin
                .CanAlterServerSettings
                .CanUseMapFunctions
                .CanPermanentlyBanPlayers
                .CanMovePlayers
                .CanIssueAllPunkbusterCommands
                .CanEditMapList
                .CanEditBanList
                .CanEditReservedSlotsList
                .CanIssueAllProconCommands
                .CanEditMapZones
                .CanEditTextChatModerationList
                .CanShutdownServer				
			*/
      }

      return isAdmin;
    }

public void ResetUpdateCheck()
{

    WritePluginConsole("[ResetUpdateCheck]", "DEBUG", 6);
    if (!isNewVersion()) CheckedOnUpdates = false; // Get Resettet from a Timer evry hour if no Update is aviable
    
}
public bool isNewVersion()
{
    return isNewVersion(GetPluginVersion());
}

public bool isNewVersion(string currentPluginVersion)
{
    WritePluginConsole("Check on Updates...", "INFO", 6);
   


    if (!CheckedOnUpdates)
    {
        WritePluginConsole("Fetch Online Version...", "DEBUG", 8);
        
        string webResult = (String)client.getWebsite();

        localversion = Regex.Match(currentPluginVersion, @"\s*([0-9])[.]([0-9])[.]([0-9])[.]([0-9])\s*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (webResult.StartsWith("System.Security.SecurityException"))
        {
            WritePluginConsole("Could not fetch Online Version. Is Sandbox enabled ??","ERROR",4);
            WritePluginConsole("Could not fetch Online Version. Please check on updates by yourselv !", "WARN", 2);
            match = Regex.Match("0.0.0.0", @"\s*([0-9][.][0-9][.][0-9][.][0-9])\s*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }
        else
        {
            match = Regex.Match(webResult, @"\s*([0-9][.][0-9][.][0-9][.][0-9])\s*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        onlineversion = Regex.Match(match.Groups[1].ToString(), @"\s*([0-9])[.]([0-9])[.]([0-9])[.]([0-9])\s*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        localV = (Convert.ToInt32(localversion.Groups[1].ToString()) * 1000) + (Convert.ToInt32(localversion.Groups[2].ToString()) * 100) + (Convert.ToInt32(localversion.Groups[3].ToString()) * 10) + (Convert.ToInt32(localversion.Groups[4].ToString()));
        onlineV = (Convert.ToInt32(onlineversion.Groups[1].ToString()) * 1000) + (Convert.ToInt32(onlineversion.Groups[2].ToString()) * 100) + (Convert.ToInt32(onlineversion.Groups[3].ToString()) * 10) + (Convert.ToInt32(onlineversion.Groups[4].ToString()));
        CheckedOnUpdates = true;

    }
    WritePluginConsole("Local Version: " + currentPluginVersion, "DEBUG", 8);
    WritePluginConsole("Online Version: " + onlineversion.Value.ToString(), "DEBUG", 8);

    if (localV < onlineV)
    {
        WritePluginConsole("========================================================", "INFO", 0);
        WritePluginConsole("UPDATE AVIABLE !", "INFO", 0);
        WritePluginConsole("-----------------------------------", "INFO", 0);
        WritePluginConsole("Update to version " + onlineversion.Value.ToString() + " now", "INFO", 0);
        WritePluginConsole("Check the download link in Plugin description", "INFO", 0);
        WritePluginConsole("========================================================", "INFO", 0);

        WritePluginEvent("UPDATE IS AVIABLE: Update to version " + onlineversion.Value.ToString() + " now", "Extra Server Funcs");
        
        return true;


    }


    return false;
}


public void sleep(int time) // Stellt den sleep befehl betreit
{
	Thread.Sleep(time);
}

public void WriteMapList(List<string> MapList)  // Schreiben einer vorgegebenen Mapliste

{

this.ServerCommand("mapList.clear");



for (int i = 0; i < MapList.Count; i++)
	{
	
	if (MapList[i] == "") return;
	string[] splitMapList = MapList[i].Split(' ');
	WritePluginConsole("Read map from list: "+i+"    " + MapList[i], "INFO", 2);
	WritePluginConsole("Splitted result 1 : "+ splitMapList[0], "DEBUG", 10);
	WritePluginConsole("Splitted result 2 : "+ splitMapList[1], "DEBUG", 10);
	WritePluginConsole("Splitted result 3 : "+ splitMapList[2], "DEBUG", 10);
	
	this.ExecuteCommand("procon.protected.send", "mapList.add", splitMapList[0], splitMapList[1], splitMapList[2]);
	this.ExecuteCommand("procon.protected.send", "mapList.save");

    this.ExecuteCommand("procon.protected.tasks.add", "Switch", "2", "1", "1", "procon.protected.send", "mapList.getMapIndices");
    this.ExecuteCommand("procon.protected.tasks.add", "Switch", "3", "1", "1", "procon.protected.send", "mapList.setNextMapIndex", "0");


	}

	
return;
}

public bool IsSwitchDefined()  // Is defined a Servermode Switch ?
{
//WritePluginConsole("Called IsSwitchDefined(): serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
if (next_serverMode != serverMode) return true;
return false;
}


public string ToMapFileName(string friendlyname)
{
    if (!isInitMapList) InitMapList();
    if (MapFileNames.ContainsKey(friendlyname)) return MapFileNames[friendlyname];
    return "";
}

public string ToFriendlyMapName(string mapfilename)
{
    try
    {
        if (!isInitMapList) InitMapList();
        if (MapFileNames.ContainsValue(mapfilename))
        {
            foreach (KeyValuePair<string, string> pair in MapFileNames)
            {
                if (pair.Value == mapfilename) return pair.Key;
            }



        }
        
    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^b[ToFriendlyMapName] returs an Error: ^0^n" + ex.ToString(), "ERROR", 4);
        WritePluginConsole("^1^b[ToFriendlyMapName] mapfilename = " + mapfilename, "DEBUG", 8);
        WritePluginConsole("^1^b[ToFriendlyMapName] Game Version = " + game_version, "DEBUG", 8);

    }
    return "";
}


public string ToPlayList(string friendlyname)
{
    if (!isInitMapList) InitMapList();
    if (GameModeNames.ContainsKey(friendlyname)) return GameModeNames[friendlyname];
    return "";
}

public string ToFriendlyModeName(string playList)
{
    try
    {
        if (!isInitMapList) InitMapList();
        if (GameModeNames.ContainsValue(playList))
        {
            foreach (KeyValuePair<string, string> pair in GameModeNames)
            {
                if (pair.Value == playList) return pair.Key;
            }



        }

    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^b[ToFriendlyModeName] returs an Error: ^0^n" + ex.ToString(), "ERROR", 4);
        WritePluginConsole("^1^b[ToFriendlyModeName] mapfilename = " + playList, "DEBUG", 8);
        WritePluginConsole("^1^b[ToFriendlyModeName] Game Version = " + game_version, "DEBUG", 8);

    }
    return "";
}




public String R(string text)  //Replacements for String Text Messages VERBESSERUNGSVORSCHLAG IN INSANE LIMITS !!!
{
   
    
    if (text.Contains("%initiator%")) text = text.Replace("%initiator%", SwitchInitiator);
    if (text.Contains("%cmd_switchnow%")) text = text.Replace("%cmd_switchnow%", switchnow_cmd);
    if (text.Contains("%cmdspeaker%")) text = text.Replace("%cmdspeaker%", lastcmdspeaker);

    if (text.Contains("%Killer%")) text = text.Replace("%Killer%", lastKiller);
    if (text.Contains("%Weapon%")) text = text.Replace("%Weapon%", lastUsedWeapon);
    if (text.Contains("%Victim%")) text = text.Replace("%Victim%", lastVictim);



    if (text.Contains("%currServermode%")) text = text.Replace("%currServermode%", R_SM(serverMode));
    if (text.Contains("%nextServermode%")) text = text.Replace("%nextServermode%", R_SM(next_serverMode));
    
    if (serverMode == "flagrun" && fm_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "flagrun" && fm_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "flagrun" && fm_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "flagrun" && fm_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "flagrun" && fm_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    if (serverMode == "knife" && kom_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "knife" && kom_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "knife" && kom_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "knife" && kom_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "knife" && kom_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    if (serverMode == "pistol" && pom_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "pistol" && pom_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "pistol" && pom_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "pistol" && pom_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "pistol" && pom_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    if (serverMode == "shotgun" && sm_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "shotgun" && sm_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "shotgun" && sm_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "shotgun" && sm_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "shotgun" && sm_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    if (serverMode == "boltaction" && bam_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "boltaction" && bam_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "boltaction" && bam_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "boltaction" && bam_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "boltaction" && bam_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    if (serverMode == "autosniper" && dmr_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
    if (serverMode == "autosniper" && dmr_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "autosniper" && dmr_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "autosniper" && dmr_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
    if (serverMode == "autosniper" && dmr_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);



    if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes && g_prohibitedWeapons.Contains(lastUsedWeapon))
    {
        if (g_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
        if (g_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

    }


    //MAP LIST PROHIBITED WEAPONS - NEW
    if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes)
    {
        if (MapProhibitedWeapons.ContainsKey(ToFriendlyMapName(currentMapFileName)))
        {
            if (MapProhibitedWeapons[ToFriendlyMapName(currentMapFileName)].Contains(lastUsedWeapon))
            {

                if (g_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
                if (g_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

            }
        }
    }



    //MODE LIST PROHIBITED WEAPONS
    if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes)
    {
        if (ModeProhibitedWeapons.ContainsKey(ToFriendlyModeName(currentGamemode)))
        {
            if (ModeProhibitedWeapons[ToFriendlyModeName(currentGamemode)].Contains(lastUsedWeapon))
            {

                if (g_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
                if (g_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
                if (g_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

            }
        }
    }

    
    return text;


}

public string R_SM(string servermode)
{
    if (servermode == "normal") return msg_normalmode;
    if (servermode == "private") return msg_privatemode;
    if (servermode == "flagrun") return msg_flagrunmode;
    if (servermode == "knife") return msg_knifemode;
    if (servermode == "pistol") return msg_pistolmode;
    if (servermode == "shotgun") return msg_shotgunmode;
    if (servermode == "boltaction") return msg_boltaction;
    if (servermode == "autosniper") return msg_autosniper;

    return servermode;
}

private void fm_Action(string name)
{
    WritePluginConsole("Called fm_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("fm_PlayerAction = " + fm_PlayerAction + " name = "+ name , "FUNCTION", 6);
    if (fm_PlayerAction == "kick") kickPlayer(name, R(msg_fmKick));
    if (fm_PlayerAction == "tban") tbanPlayer(name, fm_ActionTbanTime, R(msg_fmKick));
    if (fm_PlayerAction == "pban") pbanPlayer(name, R(msg_fmKick));
    if (fm_PlayerAction == "pb_tban") pb_tbanPlayer(name, fm_ActionTbanTime, R(msg_fmKick));
    if (fm_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_fmKick));
	
}

private void g_Action(string name)
{
    WritePluginConsole("Called g_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("fm_PlayerAction = " + g_PlayerAction + " name = "+ name , "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_prohibitedWeaponKickPlayer));
    if (g_PlayerAction == "tban") tbanPlayer(name, g_ActionTbanTime, R(msg_prohibitedWeaponKickPlayer));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_prohibitedWeaponKickPlayer));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, g_ActionTbanTime, R(msg_prohibitedWeaponKickPlayer));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_prohibitedWeaponKickPlayer));
	
}

private void mpw_Action(string name)
{
    WritePluginConsole("Called mpw_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("mpw_PlayerAction = " + mpw_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (mpw_PlayerAction == "kick") kickPlayer(name, R(msg_prohibitedWeaponKickPlayer));
    if (mpw_PlayerAction == "tban") tbanPlayer(name, g_ActionTbanTime, R(msg_prohibitedWeaponKickPlayer));
    if (mpw_PlayerAction == "pban") pbanPlayer(name, R(msg_prohibitedWeaponKickPlayer));
    if (mpw_PlayerAction == "pb_tban") pb_tbanPlayer(name, g_ActionTbanTime, R(msg_prohibitedWeaponKickPlayer));
    if (mpw_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_prohibitedWeaponKickPlayer));

}


private void kom_Action(string name)
{
    WritePluginConsole("Called kom_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("kom_PlayerAction = " + kom_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_komKick));
    if (g_PlayerAction == "tban") tbanPlayer(name, kom_ActionTbanTime, R(msg_komKick));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_komKick));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, kom_ActionTbanTime, R(msg_komKick));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_komKick));

}

private void pom_Action(string name)
{
    WritePluginConsole("Called pom_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("pom_PlayerAction = " + pom_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_pomKick));
    if (g_PlayerAction == "tban") tbanPlayer(name, pom_ActionTbanTime, R(msg_pomKick));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_pomKick));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, pom_ActionTbanTime, R(msg_pomKick));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_pomKick));

}

private void sm_Action(string name)
{
    WritePluginConsole("Called pom_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("pom_PlayerAction = " + sm_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_smKick));
    if (g_PlayerAction == "tban") tbanPlayer(name, sm_ActionTbanTime, R(msg_smKick));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_smKick));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, sm_ActionTbanTime, R(msg_smKick));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_smKick));

}

private void bam_Action(string name)
{
    WritePluginConsole("Called pom_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("pom_PlayerAction = " + bam_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_baKick));
    if (g_PlayerAction == "tban") tbanPlayer(name, bam_ActionTbanTime, R(msg_baKick));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_baKick));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, bam_ActionTbanTime, R(msg_baKick));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_baKick));

}

private void dmr_Action(string name)
{
    WritePluginConsole("Called pom_Action(" + name + ")", "FUNCTION", 6);
    WritePluginConsole("pom_PlayerAction = " + dmr_PlayerAction + " name = " + name, "FUNCTION", 6);
    if (g_PlayerAction == "kick") kickPlayer(name, R(msg_dmrKick));
    if (g_PlayerAction == "tban") tbanPlayer(name, dmr_ActionTbanTime, R(msg_dmrKick));
    if (g_PlayerAction == "pban") pbanPlayer(name, R(msg_dmrKick));
    if (g_PlayerAction == "pb_tban") pb_tbanPlayer(name, dmr_ActionTbanTime, R(msg_dmrKick));
    if (g_PlayerAction == "pb_pban") pb_pbanPlayer(name, R(msg_dmrKick));

}





    


public String E(String text)
        {
            text = Regex.Replace(text, @"\\n", "\n");
            text = Regex.Replace(text, @"\\t", "\t");
            return text;
        }

public String StripModifiers(String text)
        {
            return Regex.Replace(text, @"\^[0-9a-zA-Z]", "");
        }

private bool SendGlobalMessage(String message) // Chatmassage to all
        {
            ServerCommand("admin.say", StripModifiers(E(message)), "all");
            return true;
        }

private void SendPlayerMessage(string name, string message) // Chatnachricht an einzelnen Spieler
        {
            if (name == null)
                return;

            ServerCommand("admin.say", StripModifiers(E(message)), "player", name);
        }

private void SendPlayerYellV(string name, string message, int duration)
{
    if (name == null)
        return;

    ServerCommand("admin.yell", StripModifiers(E(message)), duration.ToString(), "player", name);
}

private bool SendGlobalYellV(String message, int duration)
{
    ServerCommand("admin.yell", StripModifiers(E(message)), duration.ToString(), "all");
    return true;
}



private List<string> CheckWeaponList(string[] WeaponListArray)
{
    List<string> tmp_WeaponList = new List<string>(WeaponListArray);
    tmp_WeaponList = CheckWeaponList(tmp_WeaponList);
    return tmp_WeaponList;

}


private List<string> CheckWeaponList(List<string> WeaponList)
{
    WritePluginConsole("[CheckWeaponList]", "DEBUG", 6);
    if (!isInitValidWeaponList) InitValidWeaponList();
    
    
    List<string> tmpWeaponList = new List<string>();

    foreach (string weapon in WeaponList)
    {
        WritePluginConsole("[CheckWeaponList] Check Weapon: " + weapon, "DEBUG", 8);
        if (!weapon.StartsWith("#"))
        {
            WritePluginConsole("[CheckWeaponList] " + weapon + " is not comment", "DEBUG", 8);
            string rweapon = weapon.Replace(" ", "");

            if (tmpWeaponList.Contains(rweapon))
            {
                WritePluginConsole("[CheckWeaponList] " + weapon + " is aleady in List", "DEBUG", 8);
            }

            else if (rweapon != "")
            {

                if (ValidWeaponList.Contains(rweapon))
                {
                    WritePluginConsole("[CheckWeaponList] " + rweapon + " is a valid Weapon Code", "DEBUG", 8);
                    tmpWeaponList.Add(rweapon);
                }
                else
                {
                    
                    if (rweapon.Length > 2)
                    {
                        WritePluginConsole("[CheckWeaponList] " + rweapon + " is no valid Weapon Code", "ERROR", 2);
                        WritePluginConsole("[CheckWeaponList]  TRY TO FIND MATCHING WEAPONCODE", "INFO", 2);
                        foreach (string validweapon in ValidWeaponList)
                        {
                            if (validweapon.IndexOf(rweapon, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                rweapon = validweapon;
                            }
                                                     
                        }

                        if (ValidWeaponList.Contains(rweapon))
                        {
                            WritePluginConsole("[CheckWeaponList] Found Matchcode " + rweapon, "INFO", 2);
                            tmpWeaponList.Add(rweapon);
                        }
                        else
                        {
                            WritePluginConsole("[CheckWeaponList] Could not find any matching Weapon. Comment out invalid entry with: #invalid#" + rweapon, "ERROR", 2);
                            tmpWeaponList.Add("#invalid#" + rweapon);
                        }


                    }
                    else
                    {
                        WritePluginConsole("[CheckWeaponList] " + rweapon + " is shorter than 3 characters.", "ERROR", 2);
                        WritePluginConsole("[CheckWeaponList] Could not find any matching Weapon. Comment out invalid entry with: #invalid#" + rweapon, "ERROR", 2);
                        tmpWeaponList.Add("#invalid#" + rweapon);

                        

                    }




                }


            }



        }

        if (weapon.StartsWith("#"))
        {
            WritePluginConsole("[CheckWeaponList] " + weapon + " is comment", "DEBUG", 8);
            tmpWeaponList.Add(weapon);
        }

    }
    return tmpWeaponList;
}


public void PreSwitchServerMode(string MarkNewServerMode) // Define a Server Mode Switch
{
    WritePluginConsole("Called PreSwitchServerMode: serverMode= " + serverMode + " next_serverMode = " + next_serverMode + "MarkNewServerMode = " + MarkNewServerMode, "Warn", 10);
    
    
    if (next_serverMode != MarkNewServerMode)
    {
        
        next_serverMode = MarkNewServerMode;

        if (MarkNewServerMode != serverMode)
        {
        
           

            if (playerCount <= 1)
            {
                StartSwitchCountdown();
            }
            else
            {
                SendSwitchMessage();
            }
        }
    }
}




public void SendSwitchMessage()
{
    WritePluginConsole("Called SendSwitchMessage(): serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
    string thread_player_message = (R(player_message));
    string thread_admin_message = R(admin_message);
    
    
    Thread delayed_message = new Thread(new ThreadStart(delegate()
		{                 
            do
			{
            if (!plugin_enabled) return;
            if (!plugin_loaded) return;
            SendGlobalMessage(thread_player_message);
			WritePluginConsole(thread_player_message, "Info", 0);
			if (SwitchInitiator != "Server") SendPlayerMessage(SwitchInitiator, thread_admin_message);  // Message to In Game Initiator of Modeswitch
			if (SwitchInitiator == "Server") WritePluginConsole(thread_admin_message, "Info", 0);		 // Initiator Message to Plugin Console
			Thread.Sleep(60 * 1000);
			if (!plugin_enabled) return;
			}while(IsSwitchDefined());
			return;
            }));
    
	delayed_message.Start();


}

public void StartSwitchCountdown()
{
    try
    {
        WritePluginConsole("Called StartSwitchCountdown()", "INFO", 10);
        if (!plugin_enabled) return;
        if (!IsSwitchDefined()) return;
        
        string countdown_message = (R(msg_countdown));
        //string countdown_message = (msg_countdown);
        int counter = countdown_time;
        int timer = 0;


        WritePluginConsole(countdown_message + " " + counter.ToString() + " SECONDS", "INFO", 1);

        while (counter >= 0)
        {


            string msg_Count = countdown_message + " " + counter.ToString() + " SECONDS";

            //procon.protected.tasks.add <int: delay> <int: interval> <int: repeat> [[vars: commandwords] ...]

            this.ExecuteCommand("procon.protected.tasks.add", "Switch", timer.ToString(), "1", "1", "procon.protected.send", "admin.yell", msg_Count, "2", "all");
           // this.ExecuteCommand("procon.protected.tasks.add", "Switch", timer.ToString(), "1", "1", "procon.protected.send", "admin.say", msg_Count, "1", "all");
           // this.ExecuteCommand("procon.protected.tasks.add", "Switch", timer.ToString(), "1", "1", "procon.protected.pluginconsole.write", msg_Count, "2", "all");



            timer++;
            counter--;
        }

        if (playerCount >= 4) // End Round if more then 3 Players are on Server
        {
            // Try to build in a Winning team detection
            this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 7).ToString(), "7", "1", "procon.protected.send", "mapList.endRound", GetWinningTeamID().ToString());
        }
        else
        {

            this.ExecuteCommand("procon.protected.tasks.add", "Switch", timer.ToString(), "1", "1", "procon.protected.plugins.call", "ExtraServerFuncs", "SwitchNow");
            this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 5).ToString(), "1", "1", "procon.protected.send", "mapList.getMapIndices");
            this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 6).ToString(), "6", "1", "procon.protected.send", "mapList.setNextMapIndex", "0");

         

            this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 7).ToString(), "7", "1", "procon.protected.send", "mapList.runNextRound");
        }
        
        
        



    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^bStartSwitchCountdown returs an Error: ^0^n" + ex.ToString(), "ERROR", 9);
    }
        //}
    
    return;
    
    


}


//Calling this method will make the settings window refresh with new data
public void UpdateSettingPage() {
	SetExternalPluginSetting("ExtraServerFuncs", "UpdateSettings", "Update");
}


public void SetPluginSetting(string key, string value)
{
    SetExternalPluginSetting("ExtraServerFuncs", key, value);
}
    
    
//Calls setVariable with the given parameters
public void SetExternalPluginSetting(String pluginName, String settingName, String settingValue)
{
    if (String.IsNullOrEmpty(pluginName) || String.IsNullOrEmpty(settingName) || settingValue == null)
    {
        WritePluginConsole("Required inputs null or empty in setExternalPluginSetting","DEBUG",8);
        return;
    }
    ExecuteCommand("procon.protected.plugins.setVariable", pluginName, settingName, settingValue);
}


private void Enable_UMM(bool onoff)
{
    if (onoff)
    {
        WritePluginConsole("Enable Ultimate Map Manager", "INFO", 6);
        this.ExecuteCommand("procon.protected.plugins.enable", "CUltimateMapManager", "true"); // Send Enable Command
    }
    else
    {
        WritePluginConsole("Disable Ultimate Map Manager", "INFO", 6);
        this.ExecuteCommand("procon.protected.plugins.enable", "CUltimateMapManager", "false"); // Send Disbale Command
    }


}




public void SwitchServerMode(string newServerMode)  // Switch the current Server Mode
{
    WritePluginConsole("Called Switch Server Mode", "Info", 10);
    WritePluginEvent("Switch Server to " + newServerMode + " mode", "Extra Server Funcs");
    if (newServerMode == "normal") 
	{
	serverMode = "normal";
	
    WriteServerConfig
	(
		nm_Servername,
		nm_Serverdescription,
		nm_ServerMessage,
		nm_MapList,
        nm_VehicleSpawnAllowed,
        nm_VehicleSpawnCount,
        nm_PlayerSpawnCount,
        nmPRoConConfig
        );
	}


    




if (newServerMode == "private")
	{
	serverMode = "private";
	
	WriteServerConfig
	(
		pm_Servername,
		pm_Serverdescription,
		pm_ServerMessage,
		pm_MapList,
        pm_VehicleSpawnAllowed,
        pm_VehicleSpawnCount,
        pm_PlayerSpawnCount,
        pmPRoConConfig
	);

	}

if (newServerMode == "flagrun")
{
    serverMode = "flagrun";

    WriteServerConfig
    (
        fm_Servername,
        fm_Serverdescription,
        fm_ServerMessage,
        fm_MapList,
        fm_VehicleSpawnAllowed,
        fm_VehicleSpawnCount,
        fm_PlayerSpawnCount,
        fmPRoConConfig // REWORK !!!!!
    );

}

if (newServerMode == "pistol")
{
    serverMode = "pistol";

    WriteServerConfig
    (
        pom_Servername,
        pom_Serverdescription,
        pom_ServerMessage,
        pom_MapList,
        pom_VehicleSpawnAllowed,
        pom_VehicleSpawnCount,
        pom_PlayerSpawnCount,
        pomPRoConConfig // REWORK !!!!!
    );

}

if (newServerMode == "knife")
{
    serverMode = "knife";

    WriteServerConfig
    (
        kom_Servername,
        kom_Serverdescription,
        kom_ServerMessage,
        kom_MapList,
        kom_VehicleSpawnAllowed,
        kom_VehicleSpawnCount,
        kom_PlayerSpawnCount,
        komPRoConConfig // REWORK !!!!!
    );

}




}

public void WriteServerConfig(string newName, string Description, string Message, List<string> NewMaplist ,enumBoolYesNo VehicleSpawnAllowed,int VehicleSpawnTime, int PlayerSpawnTime, List<string> PRoConConfig)  // Write the Config to the Server
{
    Thread thread_writeserverconfig = new Thread(new ThreadStart(delegate()
        {
            this.WritePluginConsole("Called WriteServerConfig(): serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
            if (autoconfig == enumBoolYesNo.Yes) tmp_autoconfig = "Yes";
            if (autoconfig == enumBoolYesNo.No) tmp_autoconfig = "No";
            this.SetPluginSetting("Autoconfig", "No" );
            Thread.Sleep(1000);

            if (!(isInstalledUMM && useUmm == enumBoolYesNo.Yes && serverMode == "normal"))
            {
                Enable_UMM(false);
                this.ServerCommand("vars.serverName", newName);                 // SET SERVER NAME
                this.ServerCommand("vars.serverDescription", Description);      // SET SERVER DESCRIPTION
                this.ServerCommand("vars.serverMessage", Message);              // SET SERVER MESSAGE

                if (advanced_mode == enumBoolYesNo.Yes)
                {
                    this.ServerCommand("vars.vehicleSpawnAllowed", enumboolToStringTrueFalse(VehicleSpawnAllowed));
                    this.ServerCommand("vars.vehicleSpawnDelay", VehicleSpawnTime.ToString());
                    this.ServerCommand("vars.playerRespawnTime", PlayerSpawnTime.ToString());

                    if (expert_mode == enumBoolYesNo.Yes) WritePRoConConfig(PRoConConfig);
                }


                Thread.Sleep(1000);
                this.WriteMapList(NewMaplist);
            }
            else
            {
                Enable_UMM(true);
                if (expert_mode == enumBoolYesNo.Yes) WritePRoConConfig(PRoConConfig);
            }
    



            
           
            Thread.Sleep(1000);
            this.SetPluginSetting("Autoconfig", tmp_autoconfig );
        }));

thread_writeserverconfig.Start();
}

private void WritePRoConConfig(List<string> ConfigList) // Write Plugin Commands to ProCon !!! ONLY FOR TEST
{
    foreach (string line in ConfigList)
    {
        WritePluginConsole("Write line to PRoCon: " + line, "DEBUG", 8);

        if (line.StartsWith("procon.protected.plugins"))
        {
            //line = line.Replace('"',string.Empty);
            List<string> commands = new List<string>();

            String[] parts = line.Split(new[] { '"' });
            
            foreach (string tmp in parts)
            {
                string newtmp = tmp.Trim();
                newtmp = newtmp.Replace("#LISTITEM#", "|");

                
                if (newtmp.Length > 0 && !newtmp.StartsWith("#")) commands.Add(newtmp);
            }

            

            if (commands.Count == 3)
            {
                this.ExecuteCommand(commands[0], commands[1], commands[2]);
                WritePluginConsole("Write line to PRoCon: " + commands[0] + " " + commands[1]+ " " + commands[2], "DEBUG", 8);
            }
            else if (commands.Count == 4)
            {
                this.ExecuteCommand(commands[0], commands[1], commands[2], commands[3]);
                WritePluginConsole("Write line to PRoCon: " + commands[0] + " " + commands[1] + " " + commands[2] + " " + commands[3], "DEBUG", 8);
            }
            else
            {
                WritePluginConsole("Unsupported count of Variables in PRoCon Config: " + line, "WARN", 2);
            }
        }






        
    }



}



private string GetCurrentServermode()
{
    WritePluginConsole("GetCurrentServermode()", "DEBUG", 10);
    WritePluginConsole("GetCurrentServermode() currServername =" + currServername, "DEBUG", 10);
    WritePluginConsole("GetCurrentServermode() currMessage =" + currServerMessage, "DEBUG", 10);
    WritePluginConsole("GetCurrentServermode() currDescription =" + currServerDescription, "DEBUG", 10);


    //if (Regex.Match(currServername, @nm_Servername).Success && Regex.Match(currServerMessage, @nm_Serverdescription).Success && Regex.Match(currServerMessage, @nm_Serverdescription).Success
    
    if (currServername == nm_Servername // Check if server is in Normal mode
     && currServerMessage == nm_ServerMessage
     && currServerDescription == nm_Serverdescription
        ) 
    {
        WritePluginConsole("GetCurrentServermode() detected ^bNORMAL MODE", "DEBUG", 10);
        return "normal"; 
    }

    if (currServername == pm_Servername // Check if Server is in Private Mode
     && currServerMessage == pm_ServerMessage
     && currServerDescription == pm_Serverdescription)
    {
        WritePluginConsole("GetCurrentServermode() detected ^bPRIVATE MODE", "DEBUG", 10);
        return "private";
    }

    if (currServername == fm_Servername // Check if Server is in Flagrun Mode
     && currServerMessage == fm_ServerMessage
     && currServerDescription == fm_Serverdescription)
    {
        WritePluginConsole("GetCurrentServermode() detected ^bFLAGRUN MODE", "DEBUG", 10);
        return "flagrun";
    }

    if (currServername == kom_Servername // Check if Server is in Knife Only Mode
     && currServerMessage == kom_ServerMessage
     && currServerDescription == kom_Serverdescription)
    {
        WritePluginConsole("GetCurrentServermode() detected ^bKNIFE ONLY MODE", "DEBUG", 10);
        return "knife";
    }

    if (currServername == pom_Servername // Check if Server is in Knife Only Mode
     && currServerMessage == pom_ServerMessage
     && currServerDescription == pom_Serverdescription)
    {
        WritePluginConsole("GetCurrentServermode() detected ^bPISTOL ONLY MODE", "DEBUG", 10);
        return "pistol";
    }


    // NO MATCH
    WritePluginConsole("GetCurrentServermode() could not detect current servermode", "DEBUG", 10);
    return "unknown";
		
}


private bool isInWhitelist(string wlPlayer)   // Erweitern!!! Die Gamemodes m??n eingef??werden
    {
		WritePluginConsole("Check if " + wlPlayer + " is in Whitelist", "Info", 2);
        
        if (Auto_Whitelist_Admins == enumBoolYesNo.Yes)
        {
            currentPrivileges = GetAccountPrivileges(wlPlayer);

            //Use ProCon Account as wlist Player
            if (currentPrivileges != null)
            {
                WritePluginConsole("Player has an ProCon Account", "Info", 5);
                return true;
            }
        }
        
		
  


        // Gerneral Whitelist
		if (mWhitelist_isEnabled  == enumBoolYesNo.Yes) // Is General Whitelist Enabled
		{
			WritePluginConsole("Check General Player Whitelist...", "Info", 5);
			if (m_PlayerWhitelist.Contains(wlPlayer)) // Is Player in General Player Whitelist
			{
				WritePluginConsole(wlPlayer + " is in General Player Whitelist", "Info", 2);
				return true;
			}

			if (m_ClanWhitelist.Count >= 1 && m_ClanWhitelist[0] != "")	// Is General Clan Whitelist NOT Empty
			{
				WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " Check General Clan Whitelist...", "Info", 5);
                if (m_ClanWhitelist.Contains(players.GetPlayerClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in General Clan Whitelist
				{
                    WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " is in General Clan Whitelist", "Info", 2);
					return true;
				}
			}
		}

		
		
		// Private Mode Whitelist
		if (serverMode == "private") // Is PRIVATE MODE Enabled
		{
			WritePluginConsole("Check PRIVATE MODE Player Whitelist...", "Info", 5);
			if (pm_PlayerWhitelist.Contains(wlPlayer)) // Is Player in Player Whitelist
			{
				WritePluginConsole(wlPlayer + " is in PRIVATE MODE Player Whitelist", "Info", 2);
				return true;
			}

			if (pm_ClanWhitelist.Count >= 1 && pm_ClanWhitelist[0] != "")	// Is Clan Whitelist NOT Empty
			{
				
				WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " Check PRIVATE MODE Clan Whitelist...", "Info", 5);
				if (pm_ClanWhitelist.Contains(players.GetPlayerClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
				{
					WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " is in PRIVATE MODE Clan Whitelist", "Info", 2);
					return true;
				}
			}
		}

        // Flagrun Mode Whitelist
        if (serverMode == "flagrun") // Is PRIVATE MODE Enabled
        {
            WritePluginConsole("Check FLAGRUN MODE Player Whitelist...", "Info", 5);
            if (fm_PlayerWhitelist.Contains(wlPlayer)) // Is Player in Player Whitelist
            {
                WritePluginConsole(wlPlayer + " is in PRIVATE MODE Player Whitelist", "Info", 2);
                return true;
            }

            if (fm_ClanWhitelist.Count >= 1 && fm_ClanWhitelist[0] != "")	// Is Clan Whitelist NOT Empty
            {
                
                WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " Check FLAGRUN MODE Clan Whitelist...", "Info", 5);
                if (fm_ClanWhitelist.Contains(players.GetPlayerClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
                {
                    WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " is in FLAGRUN MODE Clan Whitelist", "Info", 2);
                    return true;
                }
            }
        }

        // Pistol Only Mode Whitelist
        if (serverMode == "pistol") // Is PRIVATE MODE Enabled
        {
            WritePluginConsole("Check PISTOL ONLY MODE Player Whitelist...", "Info", 5);
            if (pom_PlayerWhitelist.Contains(wlPlayer)) // Is Player in Player Whitelist
            {
                WritePluginConsole(wlPlayer + " is in PISTOL ONLY MODE Player Whitelist", "Info", 2);
                return true;
            }

            if (pom_ClanWhitelist.Count >= 1 && pom_ClanWhitelist[0] != "")	// Is Clan Whitelist NOT Empty
            {
                
                WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " Check PISTOL ONLY MODE Clan Whitelist...", "Info", 5);
                if (pom_ClanWhitelist.Contains(players.GetPlayerClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
                {
                    WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " is in PISTOL ONLY MODE Clan Whitelist", "Info", 2);
                    return true;
                }
            }
        }


        // Knife Only Mode Whitelist
        if (serverMode == "knife") // Is PRIVATE MODE Enabled
        {
            WritePluginConsole("Check KNIFE ONLY MODE Player Whitelist...", "Info", 5);
            if (kom_PlayerWhitelist.Contains(wlPlayer)) // Is Player in Player Whitelist
            {
                WritePluginConsole(wlPlayer + " is in KNIFE ONLY MODE Player Whitelist", "Info", 2);
                return true;
            }

            if (kom_ClanWhitelist.Count >= 1 && kom_ClanWhitelist[0] != "")	// Is Clan Whitelist NOT Empty
            {
                
                WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " Check KNIFE ONLY MODE Clan Whitelist...", "Info", 5);
                if (kom_ClanWhitelist.Contains(players.GetPlayerClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
                {
                    WritePluginConsole(wlPlayer + " has Clantag: " + players.GetPlayerClanTag(wlPlayer) + " is in KNIFE ONLY MODE Clan Whitelist", "Info", 2);
                    return true;
                }
            }
        }








	WritePluginConsole(wlPlayer + " is not in Whitelist", "Info", 2);
	return false;
    }

public bool ListsEqual(List<MaplistEntry> a, List<MaplistEntry> b) // Vergleich und Schreiben der Mapliste
        {

	



		WritePluginConsole("Current MAPLIST ~~~~~~~~~~~~~~~", "INFO", 5);


			tmp_mapList = "";
			
			for (int i = 0; i < a.Count; i++)
            {
                    
                    WritePluginConsole(a[i].MapFileName + " " + a[i].Gamemode + " " + a[i].Rounds, "Info", 5);
					tmp_mapList = tmp_mapList + a[i].MapFileName + " " + a[i].Gamemode + " " + a[i].Rounds;
					if ( i < a.Count - 1) tmp_mapList = tmp_mapList + "|";
					
					
			}

			if (autoconfig == enumBoolYesNo.Yes || readconfig) 
			{
				if (serverMode == "normal") SetPluginSetting("NM_MapList", tmp_mapList );  // SAVE MAPLIST TO NORMAL MODE
				if (serverMode == "private") SetPluginSetting("PM_MapList", tmp_mapList );  // SAVE MAPLIST TO PRIVATE MODE
                if (serverMode == "flagrun") SetPluginSetting("FM_MapList", tmp_mapList);  // SAVE MAPLIST TO FLAGRUN MODE
                if (serverMode == "knife") SetPluginSetting("KOM_MapList", tmp_mapList);  // SAVE MAPLIST TO KNIFE ONLY MODE
                if (serverMode == "pistol") SetPluginSetting("POM_MapList", tmp_mapList);  // SAVE MAPLIST TO PISTOL ONLY MODE
			}
		
			
			
			
			if (a.Count != b.Count)
            {
                WritePluginConsole("Maplist counts not equal: " + a.Count + " != " + b.Count, "Info", 5);
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i].MapFileName != b[i].MapFileName || a[i].Gamemode != b[i].Gamemode || a[i].Index != b[i].Index || a[i].Rounds != b[i].Rounds)
                {
                    WritePluginConsole("Maps not equal @ " + i + " A " + a[i].MapFileName + " " + a[i].Gamemode + " " + a[i].Rounds, "Info", 5);
                    WritePluginConsole("Maps not equal @ " + i + " B " + b[i].MapFileName + " " + b[i].Gamemode + " " + b[i].Rounds, "Info", 5);
                    return false;
                }
            }

            return true;
        }

public void WritePluginConsole(string message, string tag, int level)
        {
            try
            {
                
                if (tag == "ERROR")
                {
                    tag = "^1" + tag;   // RED
                }
                else if (tag == "DEBUG")
                {
                    tag = "^3" + tag;   // ORAGNE
                }
                else if (tag == "INFO")
                {
                    tag = "^2" + tag;   // GREEN
                }
                else if (tag == "VARIABLE")
                {
                    tag = "^6" + tag;   // GREEN
                }
                else if (tag == "WARN")
                {
                    tag = "^7" + tag;   // PINK
                }


                else
                {
                    tag = "^5" + tag;   // BLUE
                }
                
                string line = "^b[" + this.GetPluginName() + "] " + tag + ": ^0^n" + message;


                if (tag == "ENABLED") line = "^b^2" + line;
                if (tag == "DISABLED") line = "^b^3"+ line;

                if (this.fDebugLevel >= 3) // WRITE LOG FILE
                {
                    files.DebugWrite(LogFileName, Regex.Replace("[" + DateTime.Now + "]" +line, "[/^][0-9bni]", "")); // L??e formatierung und schreibe in Logdatei
                }


                if (this.fDebugLevel >= level)
                {
                    this.ExecuteCommand("procon.protected.pluginconsole.write", line);
                }

            }
            catch (Exception e)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", "^1^b[" + GetPluginName() + "][ERROR]^n WritePluginConsole: ^0" + e);
            }
            
        }

public void WritePluginEvent(string text, string sender)
{

    WritePluginConsole("Write Event: " + text + " Sender: "+ sender,"DEBUG" , 6 );
    this.ExecuteCommand("procon.protected.events.write", "Plugins", "PluginAction" , text, sender);

}





public void ServerCommand(params String[] args)
{
	List<string> list = new List<string>();
	list.Add("procon.protected.send");
	list.AddRange(args);
	this.ExecuteCommand(list.ToArray());
}

#region Plugin Beschreibung und Hilfetext

public string GetPluginName() {
	return "Extra Server Funcs";
}

public string GetPluginVersion() {
	return "0.0.3.1";
}

public string GetPluginAuthor() {
	return "MarkusSR1984";
}

public string GetPluginWebsite() {
    return "";
}

public string GetPluginDescription() {

string Description = @"


If you find this plugin useful, please consider supporting me. Donations help support the servers used for development and provide incentive for additional features and new plugins! Any amount would be appreciated!</p>

<center>
<form action=""https://www.paypal.com/cgi-bin/webscr"" method=""post"" target=""_blank"">
<input type=""hidden"" name=""cmd"" value=""_s-xclick"">
<input type=""hidden"" name=""hosted_button_id"" value=""4VYFL94U9ME8L"">
<input type=""image"" src=""https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif"" border=""0"" name=""submit"" alt=""PayPal - The safer, easier way to pay online!"">
<img alt="""" border=""0"" src=""https://www.paypalobjects.com/de_DE/i/scr/pixel.gif"" width=""1"" height=""1"">
</form>
</center>



<h2>Description</h2>
This Plugin provides some functionality like Private Mode, Pistol Only Mode,  Knife Only Mode and something else. You can set various settings for each Servermode e.G. Should vehicles be allowed or not. Witch pistols should be allowed on Pistol Only Mode, All Time prohibited weapons. You can set a extra Maplist for each Servermode. The !rules command is also supported from this plugin to provide a own ruleset for each mode.  This plugin provedes a Autoconfig Method. With this its able to register some config changes in Procon and save this to plugin config. e.G. you set a new servername in Procon and the plugin change the servername also. In future it should be able to change the modes timebased or on playercounts. If i had done this i work on more modes and more settings to provide switches between normal and hardcore and something else.
</p>
If you want to automate your configured servermodes on Player Count, Time, Weekday or on a specific Day use my ""Extra Task Manager"" plugin!
Download it from: <a href=""https://github.com/MarkusSR1984/ExtraTaskPlaner/releases"" target=""_blank"">Extra Task Manager</a>
</p>
Keep your Copy up to Date and get the latest Version of Extra Server Funcs here: <a href=""https://github.com/GladiusGloriae/ExtraServerFuncs1/releases"" target=""_blank"">Extra Server Funcs latest version</a>


<blockquote><h4>NORMAL MODE</h4>
The server runs normaly and public<br/>
</blockquote>

<blockquote><h4>PRIVATE MODE</h4>
The server runs in an private mode. Only players you added to the whitelist can join the server. Player who are on your server before u activate this mode do not get kicked automaticly. You can use this mode to train with your clanmates without being distrubt from other players. Or whatever u want. This mode disables ALL types of Weapon punishment!!!<br/>
</blockquote>

<blockquote><h4>PISTOL ONLY MODE</h4>
Easy set te pistols u want to allow in the plugin settings. and please remember to write some weapons you do not allow in the rules field<br/>
</blockquote>

<blockquote><h4>KNIFE ONLY MODE</h4>
Only Knife is allowed on this servermode<br/>
</blockquote>";

if (fm_isEnabled == enumBoolYesNo.Yes) // TEST
{
Description = Description + @"
<blockquote><h4>FLAGRUN MODE</h4>
On this mode no kills are allowed. You can use this to fill your server in late hours or if u want to play this mode<br/>
</blockquote>";
}
Description = Description + @"

<h2>In Game Commands</h2>
<p>Please notice that i show you the default config here. All commands are editable in the Plugin setting. If you had done this, this commands would not work together. </p>
<blockquote><h4>!normal</h4>
Select NORMAL MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!private</h4>
Select PRIVATE MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!pistol</h4>
Select PISTOL ONLY MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!knife</h4>
Select KNIFE ONLY MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!flagrun</h4>
Select FLAGRUN MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!switchnow</h4>
Starts a countdown and switch to next Servermode without waiting on end of round.<br/>
</blockquote>

<blockquote><h4>!kickall</h4>
Kicks all player who are not in Whitelist or have an ProCon Account<br/>
</blockquote>

<blockquote><h4>!rules</h4>
Show the Servermode specific rules.<br/>
</blockquote>

<blockquote><h4>!abort</h4>
Cancel an running countdown and an defined Server Mode Switch<br/>
</blockquote>
</p>

<h2>Settings</h2>

<blockquote><h4>1. Basic Settings</h4>
<br/>
<h5>I have read the Terms of Use</h5>
Before u accept this you shuld know what you are doing, and also know the DICE Rules for Gameservers!!!!<br/>
<br/>
<h5>Private Mode</h5>
Enable or disable the ability to use the private mode<br/>
<br/>
<h5>Flagrun Mode Mode</h5>
Enable or disable the ability to use the flagrun mode<br/>
<br/>
<h5>Knife Only Mode</h5>
Enable or disable the ability to use the knife only mode<br/>
<br/>
<h5>Pistol only Mode</h5>
Enable or disable the ability to use the pistol only mode<br/>
<br/>
<h5>Use General Whitelist</h5>
Enable the ability to use general used whitelists. This lists take effect on ALL Servermodes. If this option is enabled u get shown 2 Lists where u can enter Playernames or Clantags<br/>
<br/>
<h5>Auto Whitelist ProCon Accounts</h5>
If you enable this option then is each Player who has an ProCon Account on your ProconLayerServer automaticly whitelisted. This equals an entry in General Whitelist<br/>
<br/>
<h5>Prevent ProconAccounts from warn</h5>
If you Enable this option, your Admins will not be killed and warned if they break a rule<br/>
<br/>
<h5>Prevent Whitelist Players from warn</h5>
If you Enable this option, your Whitelisted Players will not be killed and warned if they break a rule<br/>
<br/>
<h5>Use General Prohibited Weapons</h5>
Enables a List where u can enter any weapon u want to prohibit on your server. this is in all modes active BUT not in Private mode<br/>
<br/>
<h5>Use Map Prohibited Weapons</h5>
Enables a List for each map where u can enter any weapon u want to prohibit on a specific map. this is in all modes active BUT not in Private mode<br/>
<br/>
<h5>General Prohibited Weapons List</h5>
In this List you can write each weapon who you want to prohibiton your server<br/>
<br/>
<h5>Prohibited Weapon Max Player Warns</h5>
Enter here the count of wans a player get before he get kicked or baned<br/>
<br/>
<h5>Prohibited Weapon Player Action</h5>
Choose if u want to kick / temporarly ban / or ban a player for using a prohibited weapon<br/>
<br/>
<h5>Prohibited Weapon TBan Minutes</h5>
Time in Minutes for the TBan action<br/>
<br/>
<h5>Startup Mode</h5>
This option is shown when u saved your Servername to prevent your Servername will be overwritten on first start of this Plugin. Choos the mode you want to start after an Gameserver restart or an Procon Layer restart. autodetect trys to switch the server to the last active mode. if this fails the server goes to Normal Mode, none is only for first startup of the plugin and should not be used in running time.<br/>
<br/>
<h5>Aggresive Startup</h5>
If you enable this, the server taken no care on players on your server when Plugin starts. Plugin will load the Servermode specific settings and restart the current round to be able to load the new maplist. If u disable this setting the plugin waits on the end of the current round if players are on your server and get active on this time<br/>
<br/>
<h5>Countdown Timer</h5>
here u can set the Time in Seconds the Countdown should run when u enter the switchnow command<br/>
before level restart or end the round<br/>
<br/>
<h5>Show Weaponcodes</h5>
Shows each kill in the Plugin Console. The Info comtains KILLER [WEAPON] VICTIM. use this to get the weaponnames you need for the prohibited weaponlists<br/>
<br/>
<h5>Plugin Autoconfig</h5>
Enable this to easy configure the Plugin. when is enabled the Gamemodes lern the config direct from ProCon. For example: You edit the Servername in Procon, and the Plugin react on this setting and store it to the current Servermode. This works for each Serversetting, also for the Maplist. Easy enable Autoconfig, edit your Maplist in Procon, click on the refrech button, and the maps will be in the Plugin settings<br/>
<br/>
<h5>Plugin Command</h5>
Thhis is a special field to give the Plugin some commands. You can enter the in game commands without an prefix to switch between servermodes without go in game<br/>
<br/>
</blockquote>

<blockquote><h4>2. and 3. Mode Specific Settings</h4>
Here u can enter a few ssettings who are working in the specific Servermode. For example, you can set the rules who are shown to player enter the !rules command. each mode has own rules. You can also set the Servername, Description, the Join Message, etc. AND you can set a own Maplist for each Servermode. The format of the Maplist is the same like your Maplist in ftp folder of your Gameserver, but the Plugin can also read it from your Procon Maplist to make it mutch easyer for u to use this maplists, i have inserted a few examples by default<br/>
</blockquote>

<blockquote><h4>4. Plugin Commands</h4>
In this section you can edit the InGame and Plugin commands if you dont like the default only the active commands will be shown.<br/>
</blockquote>

<blockquote><h4>5. Plugin Messages</h4>
In this section you can edit the Messages who Plugin gives out to Players and admins. it`s able to use some replacements in this messags<br/>
<br/>
<b>USEABLE REPLACEMANTS</b><br/>
<pre>
%initiator%         Name of the admin who defined a Servermode change
%cmd_switchnow%     switchnow command without !/@//
%cmdspeaker%        Name of the admin who write the last command
%currServermode%    Current Server Mode
%nextServermode%    Next Server Mode
%Killer%            Last Killer
%Weapon%            Last used Weapon
%Victim%            Last Victim
%kickban%           Is Action Kick or Ban
</pre>
</blockquote>

<blockquote><h4>6.1 Map Prohibited Weapons</h4>
In this section you find some lists where you can enter the weapons who should be prohibited only on this map. You have a list for each map<br/>
</blockquote>

<blockquote><h4>6.2 Gamemode Prohibited Weapons</h4>
In this section you find some lists where you can enter the weapons who should be prohibited only on this gamemode. You have a list for each gamemode<br/>
</blockquote>

<blockquote><h4>7. Debug</h4>
In this option you can set the Debug Level. Do not do this if you have no problems with running this Plugin. If you think you found any issue than set debug level to 3 and it will be saved a logfile in the Plugin folder on your ProCon Layer. When u know what you had done when find the issue please repeat this while debug level is 3 and send me the Logfile (ExtraServerFuncs.log) with an description what you have done and what happens. Only whith this method i`m able to locate the issues and fix them. thanks for your support<br/>
</blockquote>

</p>




<h2>Changelog</h2>
<blockquote><h4>0.0.3.1 (23-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Added Support for Ultimate Map Manager !! NOT TESTED AT THE MOMENT, BECAUSE I DO NOT GET A COPY FROM DEVELOPER<br/>
	- Please give me feedback if it works<br/>
</blockquote>

<blockquote><h4>0.0.3.0 (23-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Get List of Pistols from PRoCon now<br/>
    - Added some methods for future Gamemodes<br/>
    - Edited Battlelog client. Now it sends header with each request<br/>
</blockquote>

<blockquote><h4>0.0.2.9 (22-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Reduced Update check on one per hour<br/>
    - Added a fail correction if ProCon can not fetch the online plugin version (e.g. in Sandboxed PRoCon)<br/>
    - Added Expert Mode <br/>
    - Added Option for AutoKick All when Server switch to Private Mode <br/>
    - Added a Rules Enable and Disable option <br/>
    - Rules get shown delayed now <br/>
    - Added Show Rules on First Spawn option  <br/>
    - Fixed the bug that Plugin State don´t get shown util user refeshed variables <br/>
    - Fixed the bug that Plugin Init can not get stopped <br/>
</blockquote>

<blockquote><h4>0.0.2.8 (21-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Changed a few little things on Update Check<br/>
    - Version to check funkionality of update check<br/>
</blockquote>

<blockquote><h4>0.0.2.7 (21-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Added Update Check and Information in Plugin Console<br/>
</blockquote>

<blockquote><h4>0.0.2.6 (20-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Fixed a lot of issues<br/>
    - Edited a few Methods to get better performance<br/>
    - Added PRoCon Event on ServerMode Switch<br/>
    - Check winning team before endRound on switchnow<br/>
    - fixed cancel and kickall commands<br/>
    - Added Advanced Mode for expert users<br/>
    - Added show current state in Plugin Variables<br/>
    - Added a dropdown menu to select next mode in Plugin Variables<br/>
    - Removed some Messages in Plugin Console<br/>
</blockquote>

<blockquote><h4>0.0.2.5 (13-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - End Round on Swichtnow Command if there are more the 3 Players are on Server<br/>
    - Switch instantly when mode is Predefined if Server is empty or only one Player is on Server<br/>
    - Added Support for Extra Task Manager<br/>
</blockquote>

<blockquote><h4>0.0.2.4 (08-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Reworked Replacements Method to get it work with new Prohibited Weapons Methods<br/>
    - fixed Medkit and Death bug in Pistol only Mode<br/>
    - added dynamic creation of entrys in StartupMode Variable<br/>
    - Added Server Restart detecton based on Server Settings and Uptime<br/>
</blockquote>

<blockquote><h4>0.0.2.3 (07-04-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Added Function to get Player Clan Tags from Lokal Database to reduce the load on Battlelog<br/>
	- Added Lists for experts use to send commands to other Plugins<br/>
	- Added SW40 to Pistol Only Mode<br/>
	- Added Game Mode Prohibited Weapons<br/>
	- Changed method for Map prohibited Weapons<br/>
	- Plugin is learning Maps and Gamemodes from Procon now<br/>
	- some changes for better performance of the Plugin<br/>
</blockquote>

<blockquote><h4>0.0.2.2 (04-03-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Corrected %Killer% replacement<br/>
    - Fixed Bug in Replacements funtion<br/>
    - Added Replacemant for Maplist prohibited Weapons<br/>
    - Fixed a bug that makes Display errors on higher Debug Level<br/>
    - Added function to Check of Weaponlists on double entrys and whitespaces<br/>
    - Use function CheckWeaponList for all weaponlists<br/>
    - Added Autocorrect and validity check to prohibited Weapon Lists<br/>
    - Marking invalid Weaponcodes with #invalid#<br/>

</blockquote>

<blockquote><h4>0.0.2.1 (20-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Fixed Bugs from Added Second Assult addition<br/>
</blockquote>

<blockquote><h4>0.0.2.0 (18-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Added Second Assult Maps<br/>
    - Edited Battlog Client to support BF3<br/>
    - Added BF3 Maps to Map Dictionary<br/>
</blockquote>

<blockquote><h4>0.0.1.9 (13-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Added Game Type Detection BF3 / BF4<br/>
    - Added BF3 Support for Knife Only Mode<br/>
    - Added BF3 Support for Pistol Only Mode<br/>
    - Register commands only if mode is activated in plugin settings<br/>
    - Modified save settings method to reload registered commands<br/>
    - Disabled CSV Player Database because ProCon Crashs when file is to large<br/>
    - Changed Kick Method in Private mode to TBan 5 Minutes, because slow loadind Players could join in Private mode whithout beeing whitelisted<br/>
</blockquote>

<blockquote><h4>0.0.1.8 (09-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - fixed some bugs<br/>
    - Added a check about whitespaces in Plugin Commands</br>
    - Added Plugin Description<br/>
    - Renamed a few Plugin Settings<br/>
</blockquote>

<blockquote><h4>0.0.1.7 (07-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - fixed some bugs<br/>
    - Added Logfile writing if Debug Level grather than 2<br/>
    - limit the ServerInfo Display - it is now show evry 10th time<br/>
    - changed the used method to react on commands<br/>
    - changed 'cancel' command to abort. cancel was incompatibel with InGameAdmin</br> 
</blockquote>

<blockquote><h4>0.0.1.6 (06-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - Reworked Plugin Init</br> 
    - Added Variable Aggresive Startup</br> 
    - Added Veriable Countdown Timer</br> 
    - Added option 'autodetect' to Startup Modes</br>
    - Added 'cancel' command to cancel a running countdown and Modeswitch</br> 
</blockquote>

<blockquote><h4>0.0.1.5 (05-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - edited first in-Game-Commands to a new method</br> 
</blockquote>

<blockquote><h4>0.0.1.4 (04-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
    - fixing the bug that Plugin load a random servermode on ProCon_Layer restart</br> 
</blockquote>

<blockquote><h4>0.0.1.3 (03-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
	- fixig many bugs<br/>
	- Added Map Prohibited Weapons<br/>
    - Added Auto whitelist Procon Accounts</br>
    - Added Prevent ProCon Accounts / Whitelist Players for warn</br> 
</blockquote>

<blockquote><h4>0.0.1.0 (02-02-2014)</h4>
	- ALPHA TESTING STATE<br/>
	- Give it out to ALPHA testers<br/>
	- Added Pistol only mode<br/>
</blockquote>

<blockquote><h4>0.0.0.1 (15-NOV-2013)</h4>
	- PRE ALPHA<br/>
	- initial development version<br/>
</blockquote>


";

return Description;
}
#endregion

public List<CPluginVariable> GetDisplayPluginVariables() // Liste der Anzuzeigenden Plugin variablen
    {     // Optionen zum erstellen der Konfigvariablen / dem Usermen??
        
        List<CPluginVariable> lstReturn = new List<CPluginVariable>();
        var random = new Random();


        // BASIC SETTINGS ##################################################################################################################



        lstReturn.Add(new CPluginVariable("1.Basic Settings|I have read the Terms of Use YES / NO", typeof(string), thermsofuse));
        


        if (thermsofuse == "YES")
        {
            

            //######## Status
            if (plugin_loaded)
            {
                string Server_mode_def = "enum.server_mode_" + random.Next(100000, 999999) + "(...";
                foreach (string tmpComm in Commands)
                {
                    Server_mode_def += "|" + tmpComm;

                }
                Server_mode_def += ")";


                lstReturn.Add(new CPluginVariable("0. Current Server State|Current Servermode", typeof(string), serverMode));
                if (IsSwitchDefined()) lstReturn.Add(new CPluginVariable("0. Current Server State|Next Servermode", typeof(string), next_serverMode));
                lstReturn.Add(new CPluginVariable("0. Current Server State|Select Servermode", Server_mode_def, "..."));
            }
            //######## Status

            //######## Ultimate Map Manager
            isInstalledUMM = IsUMM_installed(); // Check if UMM is Installed

            if (isInstalledUMM)
            {
                lstReturn.Add(new CPluginVariable("0.1 Ultimate Map Manager Compatibilty|Use UMM for Normal Mode", typeof(enumBoolYesNo), useUmm));
                
            }

            
            
            //######## Ultimate Map Manager


            lstReturn.Add(new CPluginVariable("1.Basic Settings|Private Mode", typeof(enumBoolYesNo), pm_isEnabled));
            if (advanced_mode == enumBoolYesNo.Yes || fm_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|Flagrun Mode", typeof(enumBoolYesNo), fm_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Knife Only Mode", typeof(enumBoolYesNo), kom_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Pistol Only Mode", typeof(enumBoolYesNo), pom_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Use General Whitelist", typeof(enumBoolYesNo), mWhitelist_isEnabled));
            if (mWhitelist_isEnabled == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Clan_Whitelist", typeof(string[]), m_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Player_Whitelist", typeof(string[]), m_PlayerWhitelist.ToArray()));
            }
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Auto Whitelist ProconAccounts", typeof(enumBoolYesNo), Auto_Whitelist_Admins));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Prevent ProconAccounts from warn", typeof(enumBoolYesNo), Prevent_Admins_Warn));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Prevent Whitelist Players from warn", typeof(enumBoolYesNo), Prevent_WlistPlayers_Warn));
                        
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Use General Prohibited Weapons", typeof(enumBoolYesNo), g_prohibitedWeapons_enable));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Use Map Prohibited Weapons", typeof(enumBoolYesNo), map_prohibitedWeapons_enable));





            if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|General Prohibited Weapons List", typeof(string[]), g_prohibitedWeapons.ToArray()));

            if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes || map_prohibitedWeapons_enable == enumBoolYesNo.Yes)
            {
                
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Prohibited Weapon Max Player Warns", typeof(int), g_max_Warns));
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Prohibited Weapon Player Action", "enum.g_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", g_PlayerAction));
                if (g_PlayerAction == "tban" || g_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("1.Basic Settings|Prohibited Weapon TBan Minutes", typeof(int), g_ActionTbanTime));
            }

            
            startup_mode_def = "enum.startup_mode_" + random.Next(100000, 999999) + "(none|autodetect|normal";
            if (pm_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|private";
            if (fm_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|flagrun";
            if (kom_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|knife";
            if (pom_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|pistol";
            startup_mode_def = startup_mode_def + ")";
            
            
            
            
            if (nm_Servername != "Your Server Name") lstReturn.Add(new CPluginVariable("1.Basic Settings|Startup Mode", startup_mode_def, startup_mode));
            if (nm_Servername != "Your Server Name") lstReturn.Add(new CPluginVariable("1.Basic Settings|Aggressive Startup", typeof(enumBoolYesNo), agresive_startup));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Countdown Timer", typeof(int), countdown_time));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Show Weaponcodes", typeof(enumBoolYesNo), showweaponcode));			

            lstReturn.Add(new CPluginVariable("1.Basic Settings|Enable Server Rules", typeof(enumBoolYesNo), rules_enable));
            if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|Show Rules in...", "enum.rules_method(chat|yell|both)", rules_method));
            if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|Show Rules on first spawn", typeof(enumBoolYesNo), rules_firstjoin));
            if (rules_enable == enumBoolYesNo.Yes && advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|Show Rules Time", typeof(int), rules_time));

            
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Plugin Autoconfig", typeof(enumBoolYesNo), autoconfig));
            if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("1.Basic Settings|Plugin Command", typeof(string), ""));
            
            

            // NORMAL MODE SETTING ##################################################################################################################



            if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Rules", typeof(string[]), nm_Rules.ToArray()));

            if (!(isInstalledUMM && useUmm == enumBoolYesNo.Yes))
            {

                lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Name", typeof(string), nm_Servername));
                lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Description", typeof(string), nm_Serverdescription));
                lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Message", typeof(string), nm_ServerMessage));
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), nm_VehicleSpawnAllowed));




                if (nm_VehicleSpawnAllowed == enumBoolYesNo.Yes)
                {
                    if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Vehicle Spawn Time", typeof(int), nm_VehicleSpawnCount));
                }
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Player Spawn Time", typeof(int), nm_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_MapList", typeof(string[]), nm_MapList.ToArray()));


            }
            else
            {
                lstReturn.Add(new CPluginVariable("2.Normal Mode|UMM was Enabled for Normal Mode", typeof(string), ""));
                lstReturn.Add(new CPluginVariable("2.Normal Mode|Disabled Normal Mode Config !!!", typeof(string), ""));
            }

            if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_PRoCon Config", typeof(string[]), nmPRoConConfig.ToArray()));
            

            
            
            // PRIVATE MODE SETTING ##################################################################################################################    
            
            

            
            
            if (pm_isEnabled == enumBoolYesNo.Yes) // PRIVATE MODE
            {

                if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Rules", typeof(string[]), pm_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Name", typeof(string), pm_Servername));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Description", typeof(string), pm_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Message", typeof(string), pm_ServerMessage));
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), pm_VehicleSpawnAllowed));

                if (pm_VehicleSpawnAllowed == enumBoolYesNo.Yes)
                {
                    if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Vehicle Spawn Time", typeof(int), pm_VehicleSpawnCount));
                }
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Player Spawn Time", typeof(int), pm_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_MapList", typeof(string[]), pm_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_ClanWhitelist", typeof(string[]), pm_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_PlayerWhitelist", typeof(string[]), pm_PlayerWhitelist.ToArray()));
                if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_PRoCon Config", typeof(string[]), pmPRoConConfig.ToArray()));

                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Autokick All on enable", typeof(enumBoolYesNo), autoKickAll));
            }

            
            
            
            // FLAGRUN MODE SETTING ##################################################################################################################    





            if (fm_isEnabled == enumBoolYesNo.Yes) // FLAGRUN MODE
            {

                if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Rules", typeof(string[]), fm_Rules.ToArray()));

                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Name", typeof(string), fm_Servername));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Description", typeof(string), fm_Serverdescription));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Message", typeof(string), fm_ServerMessage));
                 if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), fm_VehicleSpawnAllowed));

                 if (fm_VehicleSpawnAllowed == enumBoolYesNo.Yes)
                 {
                     if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Vehicle Spawn Time", typeof(int), fm_VehicleSpawnCount));
                 }
                 if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Player Spawn Time", typeof(int), fm_PlayerSpawnCount));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_MapList", typeof(string[]), fm_MapList.ToArray()));

                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_ClanWhitelist", typeof(string[]), fm_ClanWhitelist.ToArray()));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_PlayerWhitelist", typeof(string[]), fm_PlayerWhitelist.ToArray()));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Max Player Warns", typeof(int), fm_max_Warns));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Player Action", "enum.fm_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", fm_PlayerAction));
                 if (fm_PlayerAction == "tban" || fm_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_TBan Minutes", typeof(int), fm_ActionTbanTime));
                 if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_PRoCon Config", typeof(string[]), fmPRoConConfig.ToArray()));
                

                 
            }

            // KNIFE ONLY MODE SETTING ##################################################################################################################    





            if (kom_isEnabled == enumBoolYesNo.Yes) // KNIFE ONLY MODE
            {

                if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Rules", typeof(string[]), kom_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Name", typeof(string), kom_Servername));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Description", typeof(string), kom_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Message", typeof(string), kom_ServerMessage));
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), kom_VehicleSpawnAllowed));

                if (kom_VehicleSpawnAllowed == enumBoolYesNo.Yes)
                {
                    if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Vehicle Spawn Time", typeof(int), kom_VehicleSpawnCount));
                }
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Player Spawn Time", typeof(int), kom_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_MapList", typeof(string[]), kom_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_ClanWhitelist", typeof(string[]), kom_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_PlayerWhitelist", typeof(string[]), kom_PlayerWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Max Player Warns", typeof(int), kom_max_Warns));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Player Action", "enum.kom_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", kom_PlayerAction));
                if (kom_PlayerAction == "tban" || kom_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_TBan Minutes", typeof(int), kom_ActionTbanTime));
                if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_PRoCon Config", typeof(string[]), komPRoConConfig.ToArray()));



            }





            // PISTOL ONLY MODE SETTING ##################################################################################################################    





            if (pom_isEnabled == enumBoolYesNo.Yes) // PISTOL ONLY MODE
            {

                if (rules_enable == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Rules", typeof(string[]), pom_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Name", typeof(string), pom_Servername));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Description", typeof(string), pom_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Message", typeof(string), pom_ServerMessage));
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), pom_VehicleSpawnAllowed));

                if (pom_VehicleSpawnAllowed == enumBoolYesNo.Yes)
                {
                    if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Vehicle Spawn Time", typeof(int), pom_VehicleSpawnCount));
                }
                if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Player Spawn Time", typeof(int), pom_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_MapList", typeof(string[]), pom_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_ClanWhitelist", typeof(string[]), pom_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_PlayerWhitelist", typeof(string[]), pom_PlayerWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Max Player Warns", typeof(int), pom_max_Warns));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Player Action", "enum.pom_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", pom_PlayerAction));
                if (pom_PlayerAction == "tban" || pom_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_TBan Minutes", typeof(int), pom_ActionTbanTime));
                if (expert_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_PRoCon Config", typeof(string[]), pomPRoConConfig.ToArray()));
                
                
                
                //PISTOLS OLD VERSION !!!!!!!!!!!!!!!!!!
                //lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|~~~~~~~~~~~~~~ OLD PISTOLS LIST ~~~~~~~~~~~~~~", typeof(string), "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"));

                //if (game_version == "BF4")
                //{

                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M9", typeof(enumBoolYesNo), pom_allowPistol_M9));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow QSZ-92", typeof(enumBoolYesNo), pom_allowPistol_QSZ92));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow MP-443", typeof(enumBoolYesNo), pom_allowPistol_MP443));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow SHORTY 12G", typeof(enumBoolYesNo), pom_allowPistol_Shorty));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow G18", typeof(enumBoolYesNo), pom_allowPistol_Glock18));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow FN57", typeof(enumBoolYesNo), pom_allowPistol_FN57));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M1911", typeof(enumBoolYesNo), pom_allowPistol_M1911));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow 93R", typeof(enumBoolYesNo), pom_allowPistol_93R));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow CZ-75", typeof(enumBoolYesNo), pom_allowPistol_CZ75));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow .44 MAGNUM", typeof(enumBoolYesNo), pom_allowPistol_Taurus44));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow COMPACT 45", typeof(enumBoolYesNo), pom_allowPistol_HK45C));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow P226", typeof(enumBoolYesNo), pom_allowPistol_P226));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M412 REX", typeof(enumBoolYesNo), pom_allowPistol_MP412Rex));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow SW40", typeof(enumBoolYesNo), pom_allowPistol_SW40));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow KNIFE", typeof(enumBoolYesNo), pom_allowPistol_Meele));
                //}

                //if (game_version == "BF3")
                //{
                
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M9", typeof(enumBoolYesNo), pom_allowPistol_M9));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow MP-443", typeof(enumBoolYesNo), pom_allowPistol_MP443));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow G18", typeof(enumBoolYesNo), pom_allowPistol_Glock18)); // G17 & G18 Because there is only one weaponconde for it
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M1911", typeof(enumBoolYesNo), pom_allowPistol_M1911));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow 93R", typeof(enumBoolYesNo), pom_allowPistol_93R));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow .44 MAGNUM", typeof(enumBoolYesNo), pom_allowPistol_Taurus44));
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M412 REX", typeof(enumBoolYesNo), pom_allowPistol_MP412Rex));
                    
                //    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow KNIFE", typeof(enumBoolYesNo), pom_allowPistol_Meele));
                //}

                // NEW VERSION ( 0.0.3.0 )

                if (!isInitWeaponDictionarys) InitWeaponDictionarys();
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|~~~~~~~~~~~~~~ PISTOLS LIST ~~~~~~~~~~~~~~", typeof(string), "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"));

                foreach (KeyValuePair<string, enumBoolYesNo> tmpWeapon in Allow_Handguns)
                {
                    lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow " + tmpWeapon.Key, typeof(enumBoolYesNo), tmpWeapon.Value));

                }

                           


            }

            // NOTICE FOR FUTURE USE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            
            //if (!Allow_Shotguns.ContainsKey(_tmpWeapon)) Allow_Shotguns.Add(_tmpWeapon, enumBoolYesNo.Yes);
            //if (!Allow_Autosniper.ContainsKey(_tmpWeapon)) Allow_Autosniper.Add(_tmpWeapon, enumBoolYesNo.Yes);
            //if (!Allow_Boltaction.ContainsKey(_tmpWeapon)) Allow_Boltaction.Add(_tmpWeapon, enumBoolYesNo.Yes);

            // NOTICE FOR FUTURE USE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            if (advanced_mode == enumBoolYesNo.Yes)
            {
                // COMMANDS ##################################################################################################################
                lstReturn.Add(new CPluginVariable("4.Plugin Commands|NM_Command Enable", typeof(string), nm_commandEnable));
                if (pm_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|PM_Command Enable", typeof(string), pm_commandEnable));
                if (kom_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|KOM_Command Enable", typeof(string), kom_commandEnable));
                if (pom_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|POM_Command Enable", typeof(string), pom_commandEnable));
                if (fm_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|FM_Command Enable", typeof(string), fm_commandEnable));
                lstReturn.Add(new CPluginVariable("4.Plugin Commands|Switchnow_Command", typeof(string), switchnow_cmd));
                lstReturn.Add(new CPluginVariable("4.Plugin Commands|Command_Kick All", typeof(string), cmd_KickAll));


                // MESSAGES ##################################################################################################################
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PlayerInfo", typeof(string), player_message));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_AdminInfo_switchnow", typeof(string), admin_message));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_SwitchNow", typeof(string), msg_switchnow));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PM Player Kick", typeof(string), msg_pmKick));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_NOT Initiator", typeof(string), msg_notInitiator));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_Switch not defined", typeof(string), msg_switchnotdefined));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_NORMAL MODE", typeof(string), msg_normalmode));
                lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PRIVATE MODE", typeof(string), msg_privatemode));

            }

            // Special Settings
            lstReturn.Add(new CPluginVariable("7. Special Settings|Debug level", fDebugLevel.GetType(), fDebugLevel));
            lstReturn.Add(new CPluginVariable("7. Special Settings|Enable Advanced Mode", typeof(enumBoolYesNo), advanced_mode));
            if (advanced_mode == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("7. Special Settings|Enable Expert Mode", typeof(enumBoolYesNo), expert_mode));
            

            // Map Prohibited Weapons ####################################################################################################
            
            
            // NEW VERSION SICE 0.0.2.3
            if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes) 
            {

                if (!isInitMapList) InitMapList();
                if (isInitMapList)
                {
                    
                    string enumAddMapNames = "enum.AddMapNames_" + random.Next(100000, 999999) + "(...";
                    string enumRemoveMapNames = "enum.RemoveMapNames_" + random.Next(100000, 999999) + "(...";
                    foreach (string map in MapNameList)
                    {
                        if (!MapProhibitedWeapons.ContainsKey(map))
                        {
                            enumAddMapNames = enumAddMapNames + "|" + map;
                        }

                        if (MapProhibitedWeapons.ContainsKey(map))
                        {
                            enumRemoveMapNames = enumRemoveMapNames + "|" + map;
                        }




                    }
                    enumAddMapNames = enumAddMapNames + ")";
                    enumRemoveMapNames = enumRemoveMapNames + ")";

                    lstReturn.Add(new CPluginVariable("6.1 On Map prohibited Weapons|Add Map...", enumAddMapNames, ""));
                    lstReturn.Add(new CPluginVariable("6.1 On Map prohibited Weapons|Remove Map...", enumRemoveMapNames, ""));
                }

                if (MapProhibitedWeapons.Count > 0)
                {
                    foreach (KeyValuePair<string, List<string>> entry in MapProhibitedWeapons)
                    {
                        string tmpvar = "6.1 On Map prohibited Weapons|" + entry.Key;
                        lstReturn.Add(new CPluginVariable(tmpvar, typeof(string[]), (entry.Value).ToArray()));

                    }
                }



            }





            // MODE PROHIBITED WEAPONS
            if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes) 
            {
                
                if (!isInitMapList) InitMapList();
                if (isInitMapList)
                {
                    
                    string enumAddGameModes = "enum.AddGameModes_" + random.Next(100000,999999) + "(...";
                    string enumRemoveGameModes = "enum.RemoveGameModes_" + random.Next(100000, 999999) + "(...";
                    foreach (string gameMode in GameModeList)
                    {
                        if (!ModeProhibitedWeapons.ContainsKey(gameMode))
                        {
                            enumAddGameModes = enumAddGameModes + "|" + gameMode;
                        }

                        if (ModeProhibitedWeapons.ContainsKey(gameMode))
                        {
                            enumRemoveGameModes = enumRemoveGameModes + "|" + gameMode;
                        }




                    }
                    enumAddGameModes = enumAddGameModes + ")";
                    enumRemoveGameModes = enumRemoveGameModes + ")";

                    lstReturn.Add(new CPluginVariable("6.2 Gamemode prohibited Weapons|Add Game Mode...", enumAddGameModes, ""));
                    lstReturn.Add(new CPluginVariable("6.2 Gamemode prohibited Weapons|Remove Game Mode...", enumRemoveGameModes, ""));
                }

                if (ModeProhibitedWeapons.Count > 0)
                {
                    foreach (KeyValuePair<string, List<string>> entry in ModeProhibitedWeapons)
                    {
                        string tmpvar = "6.2 Gamemode prohibited Weapons|" + entry.Key;
                        lstReturn.Add(new CPluginVariable(tmpvar, typeof(string[]), (entry.Value).ToArray()));

                    }
                }



            }



                                  

		}
		return lstReturn;
	}




public List<CPluginVariable> GetPluginVariables()  // Liste der Plugin Variablen
{
    
   

    return GetDisplayPluginVariables();



} 




public void SetPluginVariable(string strVariable, string strValue) {

    if (Regex.Match(strVariable, @"ExtraTaskPlaner_Callback").Success) // Extra Task Manager Callback
    {
        ExtraTaskPlaner_Callback(strValue);
    }

    if (Regex.Match(strVariable, @"Select Servermode").Success) // Extra Task Manager Callback
    {
        if (strValue != "...") ExtraTaskPlaner_Callback(strValue);
    }



    if (strVariable.Contains("|"))
    {
        string[] tmpVariable = strVariable.Split('|');
        strVariable = tmpVariable[1];
    }

    if (plugin_loaded) WritePluginConsole("^b" + strVariable + "^n ( " + strValue + " )","VARIABLE", 10);

    if (!tmpPluginVariables.ContainsKey(strVariable)) tmpPluginVariables.Add(strVariable, strValue);

    // COMMANDS

        
    if (Regex.Match(strVariable, @"Switchnow_Command").Success)
    {
        switchnow_cmd = strValue;
    }


    if (Regex.Match(strVariable, @"Command_Kick All").Success)
    {
        cmd_KickAll = strValue;
    }

    // UMM Variables

    
    if (Regex.Match(strVariable, @"Use UMM for Normal Mode").Success)
    {
        if (strValue == "Yes")
        {
            useUmm = enumBoolYesNo.Yes;
            if ( serverMode == "normal" ) Enable_UMM(true);
        }
        if (strValue == "No")
        {
            useUmm = enumBoolYesNo.No;
            if (serverMode == "normal") Enable_UMM(false);
        }
    }
    
    
    // VARS





    if (Regex.Match(strVariable, @"I have read the Terms of Use YES / NO").Success)
    {               
        thermsofuse = strValue;
    }

    
    
    if (Regex.Match(strVariable, @"Debug level").Success) {
		int tmp = 2;
		int.TryParse(strValue, out tmp);
		fDebugLevel = tmp;
	}

    if (Regex.Match(strVariable, @"Enable Advanced Mode").Success)
    {
        if (strValue == "Yes") advanced_mode = enumBoolYesNo.Yes;
        if (strValue == "No")
        {
            advanced_mode = enumBoolYesNo.No;
            expert_mode = enumBoolYesNo.No; // Disable Expert Mode if advanced mode is off
        }
    }

    if (Regex.Match(strVariable, @"Enable Expert Mode").Success)
    {
        if (strValue == "Yes") expert_mode = enumBoolYesNo.Yes;
        if (strValue == "No") expert_mode = enumBoolYesNo.No;
    }


	if (Regex.Match(strVariable, @"Plugin Command").Success)
	{
    PluginCommand(strValue);
    }
	

    
    if (Regex.Match(strVariable, @"Startup Mode").Success)
    {
        if (strValue == "") strValue = "normal"; // Standardwert setzen
        startup_mode = strValue;
    }

      
    
    if (Regex.Match(strVariable, @"Aggressive Startup").Success)
    {
        if (strValue == "Yes") agresive_startup = enumBoolYesNo.Yes;
        if (strValue == "No") agresive_startup = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"Countdown Timer").Success)
    {
        int tmp = 2;
        int.TryParse(strValue, out tmp);
        countdown_time = tmp;
    }
    

    if (Regex.Match(strVariable, @"Show Weaponcodes").Success)
    {
        if (strValue == "Yes") showweaponcode = enumBoolYesNo.Yes;
        if (strValue == "No") showweaponcode = enumBoolYesNo.No;
    }



	if (Regex.Match(strVariable, @"Plugin Autoconfig").Success)
    {
        if (strValue == "Yes") autoconfig = enumBoolYesNo.Yes;
        if (strValue == "No") autoconfig = enumBoolYesNo.No;
    }
	
    if (Regex.Match(strVariable, @"Use General Whitelist").Success)
    {

        if (strValue == "Yes") mWhitelist_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") mWhitelist_isEnabled = enumBoolYesNo.No;

    }


    if (Regex.Match(strVariable, @"Auto Whitelist ProconAccounts").Success)
    {

        if (strValue == "Yes") Auto_Whitelist_Admins = enumBoolYesNo.Yes;
        if (strValue == "No") Auto_Whitelist_Admins = enumBoolYesNo.No;

    }




    if (Regex.Match(strVariable, @"Prevent ProconAccounts from warn").Success)
    {

        if (strValue == "Yes") Prevent_Admins_Warn = enumBoolYesNo.Yes;
        if (strValue == "No") Prevent_Admins_Warn = enumBoolYesNo.No;

    }



    if (Regex.Match(strVariable, @"Prevent Whitelist Players from warn").Success)
    {

        if (strValue == "Yes") Prevent_WlistPlayers_Warn = enumBoolYesNo.Yes;
        if (strValue == "No") Prevent_WlistPlayers_Warn = enumBoolYesNo.No;

    }






    if (Regex.Match(strVariable, @"Use General Prohibited Weapons").Success)
    {

        if (strValue == "Yes") g_prohibitedWeapons_enable = enumBoolYesNo.Yes;
        if (strValue == "No") g_prohibitedWeapons_enable = enumBoolYesNo.No;

    }





    if (Regex.Match(strVariable, @"Use Map Prohibited Weapons").Success)
    {

        if (strValue == "Yes") map_prohibitedWeapons_enable = enumBoolYesNo.Yes;
        if (strValue == "No") map_prohibitedWeapons_enable = enumBoolYesNo.No;

    }





        if (Regex.Match(strVariable, @"General Prohibited Weapons List").Success)
    {
            g_prohibitedWeapons = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
            
    }

        
    
    if (Regex.Match(strVariable, @"Clan_Whitelist").Success)
    {
		m_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }
	
	if (strVariable == "Player_Whitelist")
    {
		m_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"Prohibited Weapon Max Player Warns").Success)
    {
        g_max_Warns = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"Prohibited Weapon Player Action").Success)
    {
        g_PlayerAction = strValue;
    }

    if (Regex.Match(strVariable, @"Prohibited Weapon TBan Minutes").Success)
    {
        g_ActionTbanTime = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"Enable Server Rules").Success)
    {
        if (strValue == "Yes") rules_enable = enumBoolYesNo.Yes;
        if (strValue == "No") rules_enable = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"Show Rules in...").Success)
    {
        rules_method = strValue;
    }
    
    
    
    if (Regex.Match(strVariable, @"Show Rules on first spawn").Success)
    {
        if (strValue == "Yes") rules_firstjoin = enumBoolYesNo.Yes;
        if (strValue == "No") rules_firstjoin = enumBoolYesNo.No;
    }
    
    
    
    if (Regex.Match(strVariable, @"Show Rules Time").Success)
    {
        rules_time = Convert.ToInt32(strValue);
    }






  // NORMAL MODE VARS
  
	
	if (Regex.Match(strVariable, @"NM_Command Enable").Success)
    {
        if (strValue == "") strValue = "normal"; // Standardwert setzen
        nm_commandEnable = strValue;
    }

	if (Regex.Match(strVariable, @"NM_Rules").Success)
    {
		nm_Rules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"NM_PRoCon Config").Success)
    {

        List<string> tmpConfigList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
        nmPRoConConfig = new List<string>();
        foreach (string line in tmpConfigList)
        {
            string tmpline = line.Replace("|","#LISTITEM#");
            nmPRoConConfig.Add(tmpline);
        }

        


    }
    


	
    if (Regex.Match(strVariable, @"NM_Server Name").Success)
    {
        nm_Servername = strValue;
    }

    if (Regex.Match(strVariable, @"NM_Server Description").Success)
    {
        nm_Serverdescription = strValue;
    }

    if (Regex.Match(strVariable, @"NM_Server Message").Success)
    {
        nm_ServerMessage = strValue;
    }

    if (Regex.Match(strVariable, @"NM_Vehicle Spawn Allowed").Success)
    {
        if (strValue == "Yes") nm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "No") nm_VehicleSpawnAllowed = enumBoolYesNo.No;
        if (strValue == "True") nm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "False") nm_VehicleSpawnAllowed = enumBoolYesNo.No;

    }

    if (Regex.Match(strVariable, @"NM_Vehicle Spawn Time").Success)
    {

        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of NM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of NM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        nm_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"NM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of NM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of NM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        nm_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"NM_MapList").Success)
    {
        nm_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }



    

    
    // PRIVATE MODE VARIABLEN
    if (Regex.Match(strVariable, @"Private Mode").Success)
	{
        UnRegisterAllCommands();
        if (strValue == "Yes") pm_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") pm_isEnabled = enumBoolYesNo.No;
        RegisterAllCommands();
        taskPlanerUpdateNeeded = true;
        
    }

    if (Regex.Match(strVariable, @"PM_Command Enable").Success)
    {
        if (strValue == "") strValue = "private"; // Standardwert setzen
        pm_commandEnable = strValue;
    }

	if (Regex.Match(strVariable, @"PM_Rules").Success)
    {
		pm_Rules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }
	
    if (Regex.Match(strVariable, @"PM_ClanWhitelist").Success)
    {
        pm_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"PM_PlayerWhitelist").Success)
    {
        pm_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"PM_PRoCon Config").Success)
    {

        List<string> tmpConfigList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
        pmPRoConConfig = new List<string>();
        foreach (string line in tmpConfigList)
        {
            string tmpline = line.Replace("|", "#LISTITEM#");
            pmPRoConConfig.Add(tmpline);
        }
        
        
        
    }


    if (Regex.Match(strVariable, @"PM_Server Name").Success)
    {
        pm_Servername = strValue;
    }

    if (Regex.Match(strVariable, @"PM_Server Description").Success)
    {
        pm_Serverdescription = strValue;
    }

    if (Regex.Match(strVariable, @"PM_Server Message").Success)
    {
        pm_ServerMessage = strValue;
    }

    if (Regex.Match(strVariable, @"PM_Vehicle Spawn Allowed").Success)
    {
        if (strValue == "Yes") pm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "No") pm_VehicleSpawnAllowed = enumBoolYesNo.No;
        if (strValue == "True") pm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "False") pm_VehicleSpawnAllowed = enumBoolYesNo.No;


    }

    if (Regex.Match(strVariable, @"PM_Vehicle Spawn Time").Success)
    {
  
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of PM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of PM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        pm_VehicleSpawnCount = tmpValue;
    } 
    
    if (Regex.Match(strVariable, @"PM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of PM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        
        
        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of PM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


       pm_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"PM_MapList").Success)
    {
        pm_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"PM_Autokick All on enable").Success)
    {
        if (strValue == "Yes") autoKickAll = enumBoolYesNo.Yes;
        if (strValue == "No") autoKickAll = enumBoolYesNo.No;
       
    }
    


    // FLAGRUN MODE VARIABLEN
    if (Regex.Match(strVariable, @"Flagrun Mode").Success)
    {
        UnRegisterAllCommands();
        if (strValue == "Yes") fm_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") fm_isEnabled = enumBoolYesNo.No;
        RegisterAllCommands();
        taskPlanerUpdateNeeded = true;
    }

    if (Regex.Match(strVariable, @"FM_PRoCon Config").Success)
    {

        List<string> tmpConfigList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
        fmPRoConConfig = new List<string>();
        foreach (string line in tmpConfigList)
        {
            string tmpline = line.Replace("|", "#LISTITEM#");
            fmPRoConConfig.Add(tmpline);
        }




    }


    if (Regex.Match(strVariable, @"FM_Command Enable").Success)
    {
        if (strValue == "") strValue = "flagrun"; // Standardwert setzen
        fm_commandEnable = strValue;
    }

    if (Regex.Match(strVariable, @"FM_Rules").Success)
    {
        fm_Rules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"FM_ClanWhitelist").Success)
    {
        fm_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"FM_PlayerWhitelist").Success)
    {
        fm_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"FM_Server Name").Success)
    {
        fm_Servername = strValue;
    }

    if (Regex.Match(strVariable, @"FM_Server Description").Success)
    {
        fm_Serverdescription = strValue;
    }

    if (Regex.Match(strVariable, @"FM_Server Message").Success)
    {
        fm_ServerMessage = strValue;
    }

    if (Regex.Match(strVariable, @"FM_Vehicle Spawn Allowed").Success)
    {
        if (strValue == "Yes") fm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "No") fm_VehicleSpawnAllowed = enumBoolYesNo.No;
        if (strValue == "True") fm_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "False") fm_VehicleSpawnAllowed = enumBoolYesNo.No;


    }

    if (Regex.Match(strVariable, @"FM_Vehicle Spawn Time").Success)
    {

        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of FM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of FM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        fm_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"FM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of PM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of FM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        fm_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"FM_MapList").Success)
    {
        fm_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"FM_Max Player Warns").Success)
    {
        fm_max_Warns = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"FM_Player Action").Success)
    {
        fm_PlayerAction = strValue;
    }

    if (Regex.Match(strVariable, @"FM_TBan Minutes").Success)
    {
        fm_ActionTbanTime = Convert.ToInt32(strValue);
    }



    // KNIFE ONLY MODE VARIABLEN
    if (Regex.Match(strVariable, @"Knife Only Mode").Success)
    {
        UnRegisterAllCommands();
        if (strValue == "Yes") kom_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") kom_isEnabled = enumBoolYesNo.No;
        RegisterAllCommands();
        taskPlanerUpdateNeeded = true;
    }
    
    if (Regex.Match(strVariable, @"KOM_PRoCon Config").Success)
    {

        List<string> tmpConfigList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
        komPRoConConfig = new List<string>();
        foreach (string line in tmpConfigList)
        {
            string tmpline = line.Replace("|", "#LISTITEM#");
            komPRoConConfig.Add(tmpline);
        }




    }


    if (Regex.Match(strVariable, @"KOM_Command Enable").Success)
    {
        if (strValue == "") strValue = "flagrun"; // Standardwert setzen
        kom_commandEnable = strValue;
    }

    if (Regex.Match(strVariable, @"KOM_Rules").Success)
    {
        kom_Rules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"KOM_ClanWhitelist").Success)
    {
        kom_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"KOM_PlayerWhitelist").Success)
    {
        kom_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"KOM_Server Name").Success)
    {
        kom_Servername = strValue;
    }

    if (Regex.Match(strVariable, @"KOM_Server Description").Success)
    {
        kom_Serverdescription = strValue;
    }

    if (Regex.Match(strVariable, @"KOM_Server Message").Success)
    {
        kom_ServerMessage = strValue;
    }

    if (Regex.Match(strVariable, @"KOM_Vehicle Spawn Allowed").Success)
    {
        if (strValue == "Yes") kom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "No") kom_VehicleSpawnAllowed = enumBoolYesNo.No;
        if (strValue == "True") kom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "False") kom_VehicleSpawnAllowed = enumBoolYesNo.No;


    }

    if (Regex.Match(strVariable, @"KOM_Vehicle Spawn Time").Success)
    {

        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of KOM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of KOM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        kom_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"KOM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of PM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of KOM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        kom_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"KOM_MapList").Success)
    {
        kom_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"KOM_Max Player Warns").Success)
    {
        kom_max_Warns = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"KOM_Player Action").Success)
    {
        kom_PlayerAction = strValue;
    }

    if (Regex.Match(strVariable, @"KOM_TBan Minutes").Success)
    {
        kom_ActionTbanTime = Convert.ToInt32(strValue);
    }




    // PISTOL ONLY MODE VARIABLEN
    if (Regex.Match(strVariable, @"Pistol Only Mode").Success)
    {
        UnRegisterAllCommands();
        if (strValue == "Yes") pom_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") pom_isEnabled = enumBoolYesNo.No;
        RegisterAllCommands();
        taskPlanerUpdateNeeded = true;
    }

    if (Regex.Match(strVariable, @"POM_PRoCon Config").Success)
    {

        List<string> tmpConfigList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
        pomPRoConConfig = new List<string>();
        foreach (string line in tmpConfigList)
        {
            string tmpline = line.Replace("|", "#LISTITEM#");
            pomPRoConConfig.Add(tmpline);
        }




    }

    if (Regex.Match(strVariable, @"POM_Command Enable").Success)
    {
        if (strValue == "") strValue = "flagrun"; // Standardwert setzen
        pom_commandEnable = strValue;
    }

    if (Regex.Match(strVariable, @"POM_Rules").Success)
    {
        pom_Rules = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"POM_ClanWhitelist").Success)
    {
        pom_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"POM_PlayerWhitelist").Success)
    {
        pom_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"POM_Server Name").Success)
    {
        pom_Servername = strValue;
    }

    if (Regex.Match(strVariable, @"POM_Server Description").Success)
    {
        pom_Serverdescription = strValue;
    }

    if (Regex.Match(strVariable, @"POM_Server Message").Success)
    {
        pom_ServerMessage = strValue;
    }

    if (Regex.Match(strVariable, @"POM_Vehicle Spawn Allowed").Success)
    {
        if (strValue == "Yes") pom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "No") pom_VehicleSpawnAllowed = enumBoolYesNo.No;
        if (strValue == "True") pom_VehicleSpawnAllowed = enumBoolYesNo.Yes;
        if (strValue == "False") pom_VehicleSpawnAllowed = enumBoolYesNo.No;


    }

    if (Regex.Match(strVariable, @"POM_Vehicle Spawn Time").Success)
    {

        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of POM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of POM_Vehicle Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }
        pom_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"POM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            WritePluginConsole("Incorrect Value of PM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            WritePluginConsole("Incorrect Value of POM_Player Spawn Time","ERROR",0);
            WritePluginConsole("this Setting have to be between 5 and 100","ERROR",0);
        }


        pom_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"POM_MapList").Success)
    {
        pom_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"POM_Max Player Warns").Success)
    {
        pom_max_Warns = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"POM_Player Action").Success)
    {
        pom_PlayerAction = strValue;
    }

    if (Regex.Match(strVariable, @"POM_TBan Minutes").Success)
    {
        pom_ActionTbanTime = Convert.ToInt32(strValue);
    }
    // NEW PISTOLS ( 0.0.3.0 )


    if (Regex.Match(strVariable, @"POM_Allow").Success)
    {
        if (!isInitWeaponDictionarys) InitWeaponDictionarys(); // Init Dictionarys if not done yet !
        string tmpVar = strVariable.Replace("POM_Allow " , "");      // Remove POM Indexer

        //if (!Allow_Handguns.ContainsKey(tmpVar)) Allow_Handguns.Add(tmpVar, enumBoolYesNo.No);  // Create entry if not exist
                
        if (Allow_Handguns.ContainsKey(tmpVar))
        {
            if (strValue == "Yes") Allow_Handguns[tmpVar] = enumBoolYesNo.Yes;
            if (strValue == "No") Allow_Handguns[tmpVar] = enumBoolYesNo.No;
        }

        
    }

  


    
    //// OLD PISTOLS
    //if (Regex.Match(strVariable, @"POM_Allow M9").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_M9 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_M9 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow QSZ-92").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_QSZ92 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_QSZ92 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow MP-443").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_MP443 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_MP443 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow SHORTY 12G").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_Shorty = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_Shorty = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow G18").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_Glock18 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_Glock18 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow FN57").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_FN57 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_FN57 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow M1911").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_M1911 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_M1911 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow 93R").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_93R = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_93R = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow CZ-75").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_CZ75 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_CZ75 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow .44 MAGNUM").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_Taurus44 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_Taurus44 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow COMPACT 45").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_HK45C = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_HK45C = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow P226").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_P226 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_P226 = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow M412 REX").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_MP412Rex = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_MP412Rex = enumBoolYesNo.No;
    //}

    //if (Regex.Match(strVariable, @"POM_Allow SW40").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_SW40 = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_SW40 = enumBoolYesNo.No;
    //}
    
    //if (Regex.Match(strVariable, @"POM_Allow KNIFE").Success)
    //{
    //    if (strValue == "Yes") pom_allowPistol_Meele = enumBoolYesNo.Yes;
    //    if (strValue == "No") pom_allowPistol_Meele = enumBoolYesNo.No;
    //}

    

    //if (Regex.Match(strVariable, @"Operation Locker").Success) OnMapProhibitedWeapons_Operation_Locker = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));

    //List<string> tmp_WeaponList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    //tmp_WeaponList = CheckWeaponList(tmp_WeaponList);
    //g_prohibitedWeapons = new List<string>(tmp_WeaponList);

    

    //if (Regex.Match(strVariable, @"Zavod 311").Success) OnMapProhibitedWeapons_Zavod_311 = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Lancang Dam").Success) OnMapProhibitedWeapons_Lancang_Dam = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Flood Zone").Success) OnMapProhibitedWeapons_Flood_Zone = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Golmud Railway").Success) OnMapProhibitedWeapons_Golmud_Railway = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Paracel Storm").Success) OnMapProhibitedWeapons_Paracel_Storm = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Hainan Resort").Success) OnMapProhibitedWeapons_Hainan_Resort = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Siege of Shanghai").Success) OnMapProhibitedWeapons_Siege_of_Shanghai = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Rogue Transmission").Success) OnMapProhibitedWeapons_Rogue_Transmission = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Dawnbreaker").Success) OnMapProhibitedWeapons_Dawnbreaker = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Silk Road").Success) OnMapProhibitedWeapons_Silk_Road = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Altai Range").Success) OnMapProhibitedWeapons_Altai_Range = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Guilin Peaks").Success) OnMapProhibitedWeapons_Guilin_Peaks = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Dragon Pass").Success) OnMapProhibitedWeapons_Dragon_Pass = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));

    //if (Regex.Match(strVariable, @"Caspian Border 2014").Success) OnMapProhibitedWeapons_Caspian = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Firestorm 2014").Success) OnMapProhibitedWeapons_Firestorm = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Golf of Oman 2014").Success) OnMapProhibitedWeapons_Oman = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    //if (Regex.Match(strVariable, @"Operation Metro 2014").Success) OnMapProhibitedWeapons_Metro = new List<string>(CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));

    // MAP PROHIBITED WEAPONS

    if (Regex.Match(strVariable, @"Add Map...").Success)
    {

        if (!isInitMapList) InitMapList();
        if (isInitMapList)
        {
            if (MapNameList.Contains(strValue))
            {
                if (!MapProhibitedWeapons.ContainsKey(strValue))
                {
                    List<string> tmpList = new List<string>();
                    MapProhibitedWeapons.Add(strValue, tmpList);
                }
            }

        }
    }

    if (isInitMapList)
    {
        if (MapNameList.Contains(strVariable))
        {
            if (!MapProhibitedWeapons.ContainsKey(strVariable))
            {
                List<string> tmpList = new List<string>();
                MapProhibitedWeapons.Add(strVariable, tmpList);
            }
        }

    }

    if (MapProhibitedWeapons.ContainsKey(strVariable))
    {
        MapProhibitedWeapons[strVariable] = (CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    }

    if (Regex.Match(strVariable, @"Remove Map...").Success)
    {
        if (MapProhibitedWeapons.ContainsKey(strValue))
        {
            MapProhibitedWeapons.Remove(strValue);
        }


    }






    // GAME MODE PROHIBITED WEAPONS
    if (Regex.Match(strVariable, @"Add Game Mode...").Success)
    {
    
    if (!isInitMapList) InitMapList();
    if (isInitMapList)
    {
        if (GameModeList.Contains(strValue))
        {
            if (!ModeProhibitedWeapons.ContainsKey(strValue))
            {
                List<string> tmpList = new List<string>();
                ModeProhibitedWeapons.Add(strValue, tmpList);
            }
        }

    }
    }

    if (isInitMapList)
    {
        if (GameModeList.Contains(strVariable))
        {
            if (!ModeProhibitedWeapons.ContainsKey(strVariable))
            {
                List<string> tmpList = new List<string>();
                ModeProhibitedWeapons.Add(strVariable, tmpList);
            }
        }

    }
    
    if (ModeProhibitedWeapons.ContainsKey(strVariable))
    {
         ModeProhibitedWeapons[strVariable] = (CheckWeaponList(CPluginVariable.DecodeStringArray(strValue)));
    }

    if (Regex.Match(strVariable, @"Remove Game Mode...").Success)
    {
        if (ModeProhibitedWeapons.ContainsKey(strValue))
        {
            ModeProhibitedWeapons.Remove(strValue);
        }


    }






} // Speichern der von User eingegebenen Variablen



public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
    plugin_enabled = false;
    plugin_loaded = false;
    firstload_sleep = true;
    this.RegisterEvents(this.GetType().Name, 
                                             "OnVersion",
                                             "OnServerInfo",
                                             "OnResponseError",
                                             "OnListPlayers",
                                             "OnPlayerJoin",
                                             "OnPlayerLeft",
                                             "OnPlayerKilled",
                                             "OnPlayerSpawned",
                                             "OnGlobalChat",
                                             "OnTeamChat",
                                             "OnSquadChat",
                                             "OnRoundOverPlayers",
                                             "OnRoundOver",
                                             "OnRoundOverTeamScores",
                                             "OnLoadingLevel",
                                             "OnLevelStarted",
                                             "OnLevelLoaded",
                                             "OnServerName",
                                             "OnMaplistList",
                                             "OnServerDescription",
                                             "OnMaplistMapInserted",
                                             "OnMaplistMapRemoved",
                                             "OnPlayerAuthenticated",
                                             "OnServerMessage",
                                             "OnVehicleSpawnAllowed",
                                             "OnVehicleSpawnDelay",
                                             "OnPlayerRespawnTime",
                                             "OnPluginLoadingEnv",
                                             "OnPlayerTeamChange"
                                             );
    // EXTRA TASK MANAGER 

    isInstalledUMM = IsUMM_installed();
    
    Thread startup_sleep = new Thread(new ThreadStart(delegate()
    {
        Thread.Sleep(2000);
        if (IsExtraTaskPlanerInstalled())
        {
            do
            {
                SendTaskPlanerInfo();
                Thread.Sleep(2000);
            }
            while (!isRegisteredInTaskPlaner);
        }
    }));
    startup_sleep.Start();
    // EXTRA TASK MANAGER 


    //// Resend Plugin Variables
    //foreach (KeyValuePair<string, string> tmpVar in tmpPluginVariables)
    //{
    //    SetPluginVariable(tmpVar.Key, tmpVar.Value);
    //}

}
private void CancelSwitch()
{
    WritePluginConsole("^1^b[CancelSwitch]", "DEBUG", 6);
    if (IsSwitchDefined())
    {
        this.ExecuteCommand("procon.protected.tasks.remove", "Switch");
        next_serverMode = serverMode;
    }

}



public void OnCommand_Cancel(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) // Funktion zum Testcommand
{
    WritePluginConsole("^1^b[OnCommand_Cancel]^0^n Speaker was: "+strSpeaker+" text was: "+strText , "Info", 10);
    if (IsSwitchDefined())
    {
        this.ExecuteCommand("procon.protected.tasks.remove", "Switch");
        
        next_serverMode = serverMode;
        SendPlayerMessage(strSpeaker, "Canceled current activity....");
    }
    else
    {
        SendPlayerMessage(strSpeaker, "There is nothing to cancel");
    }
}



public void OnPluginEnable() 
{
    stop_init = false;
    WritePluginConsole(" - Thanks for using :)", "ENABLED", 0);
    InitPlugin();
    this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncs_CheckUpdate_Intervall", "3600", "3600" , "-1", "procon.protected.plugins.call", "ExtraServerFuncs", "ResetUpdateCheck"); // Check Online Version evry 60 Minutes
}
public void InitPlugin()
{
    try
    {
        Thread thread_PluginEnable = new Thread(new ThreadStart(delegate()
        {
                        
            Thread.Sleep(2000);
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working
            
            Enable_UMM(true);
            Thread.Sleep(500);
            isInstalledUMM = IsUMM_installed();

            if (isInstalledUMM) Enable_UMM(false);

            Thread.Sleep(2000);

            if (stop_init) return; // Chek if Plugin gets Disabled and stop working

            if (thermsofuse == "YES")
            {
                WritePluginConsole("User acctepted the therms of use... run Plugin init...", "INFO", 0);
                
            }
            else
            {
                WritePluginConsole("You have to accept the therms of use before you can use this plugin", "WARN", 0);
                this.ExecuteCommand("procon.protected.plugins.enable", "ExtraServerFuncs", "false"); // Send Disable Command
                return;
            }
            
            
            
            
            WritePluginConsole("Init Plugin...", "INFO", 0);
            Thread.Sleep(2000);
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working



            if (firstload_sleep)
            {
                serverMode = "first_start";
                next_serverMode = "first_start";
                WritePluginConsole("^1^bENABLE PLUGIN FIRST TIME! SLEEP FOR 30 SECONDS", "INFO", 2);
                Thread.Sleep(30000); // Wenn Procon das erste mal gestartet wird 30 sekunden Warten
                firstload_sleep = false;
            }
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working
            

            WritePluginConsole("Set Startup Vars...", "INFO", 2);
            plugin_enabled = true;
            serverInfoloaded = false;


            players = new PlayerDB();
            files = new TextDatei();
            
            fIsEnabled = true;

            Thread.Sleep(1000);
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working
            WritePluginConsole("Update Current Server Variables", "DEBUG", 10);
            UdateServerConfig();
            Thread.Sleep(1000);
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working


            serverMode = "plugin_init";
            
            WritePluginConsole("startup_mode = " + startup_mode, "DEBUG", 10);
            
            if (startup_mode == "none")
            {
                serverMode = "normal";
                next_serverMode = "normal";
            }
            else if (startup_mode == "autodetect") // AUTODETECT CURRENT SERVER MODE
            {
                string tmp_Servermode = GetCurrentServermode();
                if (tmp_Servermode == "unknown")
                {
                    serverMode = "plugin_init";
                    next_serverMode = "normal";
                }
                else
                {
                    serverMode = tmp_Servermode;
                    next_serverMode = tmp_Servermode;
                }

            }
            else
            {
                next_serverMode = startup_mode; // Setze den n?ten Servermode auf Startup mode
            }


            



            WritePluginConsole("^3Wait on: OnServerInfo()", "DEBUG", 10);
            
            while (!serverInfoloaded && !stop_init) { Thread.Sleep(500); } // Wait on Server Info
            
            if (stop_init) return; // Chek if Plugin gets Disabled and stop working
            
            WritePluginConsole("^2Recived: OnServerInfo()", "DEBUG", 10);


            WritePluginConsole("playerCount =" + playerCount, "DEBUG", 10);

            if (startup_mode != "none")
            {
                WritePluginConsole("startup mode is not 'none'", "DEBUG", 10);
                if (playerCount < 1)   
                {
                    WritePluginConsole("Player Count is less than 1", "DEBUG", 10);
                    serverMode = "plugin_init";
                    PreSwitchServerMode(next_serverMode);
                    StartSwitchCountdown();
                }
                else if (agresive_startup == enumBoolYesNo.Yes)                  
                {            
                    WritePluginConsole("Aggresive Startup is ON", "DEBUG", 10);
                    PreSwitchServerMode(next_serverMode);
                    StartSwitchCountdown();
                }


            }
            else
            {
                PreSwitchServerMode(next_serverMode);
                
            }

            

            WritePluginConsole("LOADED Startup Server Mode: " + next_serverMode, "INFO", 2);
            WritePluginConsole("Register Commands", "INFO", 10);
            RegisterAllCommands();

            plugin_loaded = true;

            isNewVersion(GetPluginVersion()); // Check on Update
            SetPluginSetting("REFRESH PLUGIN VARIABLES", ""); // Refresh to get shown State Display after Init
            WritePluginConsole("....ready! Do my work now :)", "INFO", 2);
            return;
            }));

        thread_PluginEnable.Start();
    }
    catch (Exception e)
    {
        WritePluginConsole("Caught Exception in InitPlugin()", "ERROR", 2);
        WritePluginConsole(e.Message, "ERROR", 2);
        throw;
    }




}

private void RegisterAllCommands()
{
    WritePluginConsole("[RegisterAllCommands]", "DEBUG", 6);
    if (rules_enable == enumBoolYesNo.Yes)
    {
        this.RegisterCommand(
                         new MatchCommand(
                             "ExtraServerFuncs",
                             "OnCommand_Rules",
                             this.Listify<string>("@", "!", "/"),
                             rules_command,
                             this.Listify<MatchArgumentFormat>(),
                             new ExecutionRequirements(
                                 ExecutionScope.All), // PUBLIC COMMAND
                             "Show current rules"
                         ));
    }
    //this.RegisterCommand(
    //                new MatchCommand(
    //                    "ExtraServerFuncs",
    //                    "OnCommand_Kickall",
    //                    this.Listify<string>("@", "!", "/"),
    //                    cmd_KickAll,
    //                    this.Listify<MatchArgumentFormat>(),
    //                    new ExecutionRequirements(
    //                        ExecutionScope.Account,
    //                        //2,
    //                        //"yes", //confirmationCommand,
    //                        "You do not have enough privileges to kick all players"),
    //                    "Kicks ALL players from server"
    //                )
    //            );

    //this.RegisterCommand(
    //                new MatchCommand(
    //                    "ExtraServerFuncs",
    //                    "OnCommand_Cancel",
    //                    this.Listify<string>("@", "!", "/"),
    //                    cmd_cancel,
    //                    this.Listify<MatchArgumentFormat>(),
    //                    new ExecutionRequirements(
    //                        ExecutionScope.Account,
    //                        //2,
    //                        //"yes", //confirmationCommand,
    //                        "You do not have enough privileges"),
    //                    "Cancels a countdown timer"
    //                )
    //            );


    this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Normal",
                        this.Listify<string>("@", "!", "/"),
                        nm_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to NORMAL MODE"
                    )
                );


if (pm_isEnabled == enumBoolYesNo.Yes)    this.RegisterCommand(
                                          new MatchCommand(
                                          "ExtraServerFuncs",
                                          "OnCommand_Private",
                                          this.Listify<string>("@", "!", "/"),
                                          pm_commandEnable,
                                          this.Listify<MatchArgumentFormat>(),
                                          new ExecutionRequirements(
                                          ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to PRIVATE MODE"
                    )
                );

if (fm_isEnabled == enumBoolYesNo.Yes)   this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Flagrun",
                        this.Listify<string>("@", "!", "/"),
                        fm_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to FLAGRUN MODE"
                    )
                );



if (kom_isEnabled == enumBoolYesNo.Yes) this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Knife",
                        this.Listify<string>("@", "!", "/"),
                        kom_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to KNIFE ONLY MODE"
                    )
                );

if (pom_isEnabled == enumBoolYesNo.Yes) this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Pistol",
                        this.Listify<string>("@", "!", "/"),
                        pom_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to PISTOL ONLY MODE"
                    )
                );

    this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Switchnow",
                        this.Listify<string>("@", "!", "/"),
                        switchnow_cmd,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Switch to the defined Servermode instantly"
                    )
                );

     
 


}

private void UnRegisterAllCommands()
{
    WritePluginConsole("[UnRegisterAllCommands]", "DEBUG", 6);
    if (rules_enable == enumBoolYesNo.Yes)
    {
        this.UnregisterCommand(
                         new MatchCommand(
                             "ExtraServerFuncs",
                             "OnCommand_Rules",
                             this.Listify<string>("@", "!", "/"),
                             rules_command,
                             this.Listify<MatchArgumentFormat>(),
                             new ExecutionRequirements(
                                 ExecutionScope.All), // PUBLIC COMMAND
                             "Show current rules"
                         ));
    }
    //this.UnregisterCommand(
    //                new MatchCommand(
    //                    "ExtraServerFuncs",
    //                    "OnCommand_Kickall",
    //                    this.Listify<string>("@", "!", "/"),
    //                    cmd_KickAll,
    //                    this.Listify<MatchArgumentFormat>(),
    //                    new ExecutionRequirements(
    //                        ExecutionScope.Account,
    //    //2,
    //    //"yes", //confirmationCommand,
    //                        "You do not have enough privileges ti kick all players"),
    //                    "Kicks ALL players from server"
    //                )
    //            );

    //this.UnregisterCommand(
    //                new MatchCommand(
    //                    "ExtraServerFuncs",
    //                    "OnCommand_Cancel",
    //                    this.Listify<string>("@", "!", "/"),
    //                    cmd_cancel,
    //                    this.Listify<MatchArgumentFormat>(),
    //                    new ExecutionRequirements(
    //                        ExecutionScope.Account,
    //    //2,
    //    //"yes", //confirmationCommand,
    //                        "You do not have enough privileges"),
    //                    "Cancels a countdown timer"
    //                )
    //            );


    this.UnregisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Normal",
                        this.Listify<string>("@", "!", "/"),
                        nm_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to NORMAL MODE"
                    )
                );


    this.UnregisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Private",
                        this.Listify<string>("@", "!", "/"),
                        pm_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to PRIVATE MODE"
                    )
                );

    this.UnregisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Flagrun",
                        this.Listify<string>("@", "!", "/"),
                        fm_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to FLAGRUN MODE"
                    )
                );



    this.UnregisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Knife",
                        this.Listify<string>("@", "!", "/"),
                        kom_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to KNIFE ONLY MODE"
                    )
                );

    this.UnregisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommand_Pistol",
                        this.Listify<string>("@", "!", "/"),
                        pom_commandEnable,
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                            "You do not have enough privileges"),
                        "Define a switch to PISTOL ONLY MODE"
                    )
                );

    this.UnregisterCommand(
                new MatchCommand(
                    "ExtraServerFuncs",
                    "OnCommand_Switchnow",
                    this.Listify<string>("@", "!", "/"),
                    switchnow_cmd,
                    this.Listify<MatchArgumentFormat>(),
                    new ExecutionRequirements(
                        ExecutionScope.Account,
        //2,
        //"yes", //confirmationCommand,
                        "You do not have enough privileges"),
                    "Switch to the defined Servermode instantly"
                )
            );


}

public void OnCommand_Rules(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    
    WritePluginConsole("OnCommand_Rules()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Rules() strSpeaker='" + strSpeaker + "' strText='" + strText + "'" , "DEBUG", 8);
    WritePluginConsole(strSpeaker + " requested the Rules", "INFO", 2);
    ShowRules(strSpeaker);
    return;

}

public void OnCommand_Kickall(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    WritePluginConsole("OnCommand_KickAll()", "DEBUG", 6);
    WritePluginConsole("OnCommand_KickAll() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    lastcmdspeaker = strSpeaker;
    KickAll();
}


public void OnCommand_Normal(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) 
{
    WritePluginConsole("OnCommand_Normal()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Normal() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    SwitchInitiator = strSpeaker;
    lastcmdspeaker = strSpeaker;
    CancelSwitch();
    PreSwitchServerMode("normal");
    return;
}

public void OnCommand_Private(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) 
{
    WritePluginConsole("OnCommand_Private()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Private() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    SwitchInitiator = strSpeaker;
    lastcmdspeaker = strSpeaker;
    CancelSwitch();
    PreSwitchServerMode("private");
    return;
}

public void OnCommand_Flagrun(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    WritePluginConsole("OnCommand_Flagrun()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Flagrun() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    SwitchInitiator = strSpeaker;
    lastcmdspeaker = strSpeaker;
    CancelSwitch();
    PreSwitchServerMode("flagrun");
    return;
}

public void OnCommand_Knife(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    WritePluginConsole("OnCommand_Knife()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Knife() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    SwitchInitiator = strSpeaker;
    lastcmdspeaker = strSpeaker;
    CancelSwitch();
    PreSwitchServerMode("knife");
    return;
}

public void OnCommand_Pistol(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    WritePluginConsole("OnCommand_Pistol()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Pistol() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    SwitchInitiator = strSpeaker;
    lastcmdspeaker = strSpeaker;
    CancelSwitch();
    PreSwitchServerMode("pistol");
    return;
}

public void OnCommand_Switchnow(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope)
{
    WritePluginConsole("OnCommand_Switchnow()", "DEBUG", 6);
    WritePluginConsole("OnCommand_Switchnow() strSpeaker='" + strSpeaker + "' strText='" + strText + "'", "DEBUG", 8);
    lastcmdspeaker = strSpeaker;
    if (IsSwitchDefined())
    {
        if (SwitchInitiator == strSpeaker) StartSwitchCountdown();
        if (SwitchInitiator != strSpeaker) SendPlayerMessage(strSpeaker, R(msg_notInitiator));
        return;
    }


    if (!IsSwitchDefined()) SendPlayerMessage(strSpeaker, R(msg_switchnotdefined));
    return;
}



public void OnPluginLoadingEnv(List<string> lstPluginEnv)
{
    foreach (String env in lstPluginEnv)
    {
        WritePluginConsole("Got ^bOnPluginLoadingEnv: " + env,"DEBUG", 6);
    }
    game_version = lstPluginEnv[1];
    WritePluginConsole("^1Game Version = " + lstPluginEnv[1] , "DEBUG",8);
}



public void OnPluginDisable() {
    WritePluginConsole("OnPluginDisable()", "DEBUG", 6);

    this.ExecuteCommand("procon.protected.tasks.remove", "Switch");
    this.ExecuteCommand("procon.protected.tasks.remove", "ExtraServerFuncs_CheckUpdate_Intervall");
    
    // Threads have to been stoped here !!!!!!!!!

    plugin_enabled = false;
    plugin_loaded = false;
    fIsEnabled = false;
    stop_init = true;
    UnRegisterAllCommands();

    WritePluginConsole("", "DISABLED", 0);

    

}

public override void OnVersion(string serverType, string version) { }

public override void OnServerInfo(CServerInfo serverInfo) {

        


        currentMapFileName = serverInfo.Map;
        currentGamemode = serverInfo.GameMode;
        currentRound = serverInfo.CurrentRound;
        totalRounds = serverInfo.TotalRounds;
        playerCount = serverInfo.PlayerCount;
        maxPlayerCount = serverInfo.MaxPlayerCount;
        ServerUptime = serverInfo.ServerUptime;    
        serverInfoloaded = true;
        
        currentTeamScores = serverInfo.TeamScores;
        
       

            //serverInfo.RoundTime;

        if (!(isInstalledUMM && useUmm == enumBoolYesNo.Yes && serverMode == "normal")) Enable_UMM(false);
    
        if (taskPlanerUpdateNeeded) SendTaskPlanerInfo();    

        ServerInfoCounter++; // Einen durchlauf zaelen
        if (ServerUptime >= 360) ServerUptimePluginHasReInit = false;

        if (plugin_enabled && ServerInfoCounter >= 20)
        {
            isNewVersion(GetPluginVersion()); // Check on Update

            if (/*(GetCurrentServermode() == "unknown" && startup_mode != "none" && plugin_loaded) ||*/ (!ServerUptimePluginHasReInit && ServerUptime <= 300 && plugin_loaded))
            {
                
                WritePluginConsole("DETECTED UNKNOWN SERVER CONFIG", "INFO", 2);
                WritePluginConsole("REINITAILZE PLUGIN TO SET STARTUP CONFIG", "INFO", 2);
                ServerUptimePluginHasReInit = true;
                InitPlugin();
            }

            WritePluginConsole("^1^bCurrent Servermode: ^0^n" + serverMode + "^1^b Next Servermode: ^0^n" + next_serverMode, "INFO", 3);
            WritePluginConsole("^4^bCurrent round: ^2^n " + ToFriendlyMapName(currentMapFileName) + "^5 PlayersCount: ^0" + playerCount, "INFO", 3);
            WritePluginConsole("DEBUG LEVEL " + fDebugLevel, "DEBUG", 3);
            ServerInfoCounter = 0;
        }   
    
}

public void OnMaplistMapInserted(int mapIndex, string mapFileName)
{
    WritePluginConsole("OnMaplistMapInserted()", "DEBUG", 10);
    if (autoconfig == enumBoolYesNo.Yes)
    {
        this.ServerCommand("mapList.list");  // Update Maplist if Autoconfig is on
        WritePluginConsole("OnMaplistMapInserted() AUTOUPDATE: mapList.list", "DEBUG", 10);
    }
}

public void OnMaplistMapRemoved(int mapIndex)
{
    WritePluginConsole("OnMaplistMapRemoved()", "DEBUG", 10);
    if (autoconfig == enumBoolYesNo.Yes)
    {
        this.ServerCommand("mapList.list");  // Update Maplist if Autoconfig is on
        WritePluginConsole("OnMaplistMapInserted() AUTOUPDATE: mapList.list", "DEBUG", 10);
    }

}



public void OnAnyChat(string speaker, string message)
{
    WritePluginConsole("OnAnyChat", "DEBUG", 6);

    if (IsCommand(message) && IsAdmin(speaker))    // Helper Method, becouse i got problems with registered command not be executed
    {
        string tmpCommand = ExtractCommand(message);
        MatchCommand emptyCommand = new MatchCommand(new List<string>(), "", new List<MatchArgumentFormat>());
        CapturedCommand emptyCapturedCommand = new CapturedCommand("", "", new List<MatchArgument>(), "");
        CPlayerSubset emptyCPlayerSubset = new CPlayerSubset(new List<string>());

        if (tmpCommand == cmd_KickAll) OnCommand_Kickall(speaker, message, emptyCommand, emptyCapturedCommand, emptyCPlayerSubset);
        if (tmpCommand == cmd_cancel) OnCommand_Cancel(speaker, message, emptyCommand, emptyCapturedCommand, emptyCPlayerSubset);

            

    }


}

public void OnServerName(string serverName)  // Server Name was changed
{
    currServername = serverName;
    
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginSetting("NM_Server Name", serverName); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Server Name", serverName); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Server Name", serverName); // SAVE SERVERNAME TO FLAGRUN MODE CONFIG
            if (serverMode == "knife") SetPluginSetting("KOM_Server Name", serverName); // SAVE SERVERNAME TO KNIFE ONLY MODE CONFIG
            if (serverMode == "pistol") SetPluginSetting("POM_Server Name", serverName); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
        }
    
}

public void OnServerMessage(string Message)  // Server Name was changed
{
    currServerMessage = Message;
    
            if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginSetting("NM_Server Message", Message); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Server Message", Message); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Server Message", Message); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "knife") SetPluginSetting("KOM_Server Message", Message); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "pistol") SetPluginSetting("POM_Server Message", Message); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
        }
    
}

public void OnServerDescription(string serverDescription)
{
    currServerDescription = serverDescription;
    
    if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginSetting("NM_Server Description", serverDescription); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Server Description", serverDescription); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Server Description", serverDescription); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "knife") SetPluginSetting("KOM_Server Description", serverDescription); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "pistol") SetPluginSetting("POM_Server Description", serverDescription); // SAVE SERVERNAME TO PRIVATE MODE CONFIG



        }
    


}

public void OnVehicleSpawnAllowed(bool isEnabled)   // vars.vehicleSpawnAllowed
{
  
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {

            if (serverMode == "normal") SetPluginSetting("NM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled)); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled)); // SAVE TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled));
            if (serverMode == "pistol") SetPluginSetting("POM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled));
            if (serverMode == "knife") SetPluginSetting("KOM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled));
        }
  

}

public void OnVehicleSpawnDelay(int limit)   // vars.vehicleSpawnDelay
{
   
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginSetting("NM_Vehicle Spawn Time", limit.ToString()); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Vehicle Spawn Time", limit.ToString()); // SAVE TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Vehicle Spawn Time", limit.ToString());
            if (serverMode == "pistol") SetPluginSetting("POM_Vehicle Spawn Time", limit.ToString());
            if (serverMode == "knife") SetPluginSetting("KOM_Vehicle Spawn Time", limit.ToString());
        }
   
}

public void OnPlayerRespawnTime(int limit)   //vars.playerRespawnTime
{
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginSetting("NM_Player Spawn Time", limit.ToString()); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginSetting("PM_Player Spawn Time", limit.ToString()); // SAVE TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginSetting("FM_Player Spawn Time", limit.ToString());
            if (serverMode == "pistol") SetPluginSetting("POM_Player Spawn Time", limit.ToString());
            if (serverMode == "knife") SetPluginSetting("KOM_Player Spawn Time", limit.ToString());
        }
   


}

public void OnMaplistList(List<MaplistEntry> lstMaplist)
{
    if (plugin_enabled)
    {
        try
        {
            if (!ListsEqual(lstMaplist, m_listCurrMapList) && m_listCurrMapList.Count > 0)
            {
                WritePluginConsole("Maplist change detected", "Info", 2);

            }
        }
     
        catch (Exception e)
        {
            WritePluginConsole("Caught Exception in ListsEqual", "ERROR", 0);
            WritePluginConsole(e.Message, "ERROR", 0);
            throw;
        }
        this.m_listCurrMapList = new List<MaplistEntry>(lstMaplist);
        WritePluginConsole("Maplist updated. There are " + m_listCurrMapList.Count + " maps currently in the maplist", "INFO", 5);

    }



}

public override void OnResponseError(List<string> requestWords, string error)
{
    WritePluginConsole("^1^bPROCON ERROR: ^0^n"+ error, "ERROR", 4);

}

public override void OnListPlayers(List<CPlayerInfo> playerlist, CPlayerSubset subset) {
    List<string> playerdb = new List<string>();
    List<string> tmpplayerdb = new List<string>();

    foreach (CPlayerInfo player in playerlist)
    {
        players.Add(player.SoldierName);
        tmpplayerdb.Add(player.SoldierName);
    }

    playerdb = players.ListPlayers();

    foreach (string player in playerdb)
    {
        if (!tmpplayerdb.Contains(player)) players.Remove(player); // Spieler L??en wenn noch in der DB aber nicht mehr auf dem Server 
    
    
    
    }
}

public override void OnPlayerJoin(string soldierName) 
{
    WritePluginConsole("[OnPlayerJoin()]", "DEBUG", 6);
    if (plugin_enabled)
    {
        if (serverMode == "private") // Check if server is in PRIVATE MODE
        {
            //if (!isInWhitelist(soldierName)) tbanPlayer(soldierName, 5, msg_pmKick);  // Kick player if not in Whitelist
            //if (!isInWhitelist(soldierName)) delayedTbanPlayer(soldierName, 5, msg_pmKick, 30);  // Kick player if not in Whitelist
            tmpBanList.Add(soldierName);

        }

    }
}

public override void OnPlayerAuthenticated(string soldierName, string guid)
{
    WritePluginConsole("[OnPlayerAuthenticated()]", "DEBUG",6);
    players.Add(soldierName);
}

public override void OnPlayerLeft(CPlayerInfo playerInfo)
{
    WritePluginConsole("[OnPlayerLeft()]", "DEBUG", 6);
    players.Remove(playerInfo.SoldierName);

    if (spawnedPlayer.Contains(playerInfo.SoldierName))
    {
        spawnedPlayer.Remove(playerInfo.SoldierName);
    }

}


public override void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
{
    WritePluginConsole("[OnPlayerTeamChange()] Name = " + soldierName + " TeamID = " + teamId.ToString() + " SquadID = " + squadId.ToString() , "DEBUG", 6);
    if (plugin_enabled)
    {
        if (serverMode == "private") // Check if server is in PRIVATE MODE
        {
            if (tmpBanList.Contains(soldierName))
            {
                if (!isInWhitelist(soldierName)) tbanPlayer(soldierName, 5, msg_pmKick);  // Kick player if not in Whitelist
                tmpBanList.Remove(soldierName);
            }
            
            //if (!isInWhitelist(soldierName)) delayedTbanPlayer(soldierName, 5, msg_pmKick, 30);  // Kick player if not in Whitelist
            
        }
        else
        {
            tmpBanList.Clear();
        }



    }

}


public override void OnPlayerKilled(Kill kKillerVictimDetails)
 {
     if (plugin_loaded)
     {
         try
         {


             lastKiller = kKillerVictimDetails.Killer.SoldierName;
             lastVictim = kKillerVictimDetails.Victim.SoldierName;
             lastWeapon = kKillerVictimDetails.DamageType;
             lastUsedWeapon = FWeaponName(lastWeapon);

             if (showweaponcode == enumBoolYesNo.Yes) WritePluginConsole("^2^n" + lastKiller + "^1^b  [ " + lastUsedWeapon + " ]^7^n " + lastVictim, "KILL", 0); // Zeige Die Waffen in der Konsole Farblich hervorgehoben


             WritePluginConsole("[OnPlayerKilled] Killer:     " + lastKiller,"DEBUG", 8);
             WritePluginConsole("[OnPlayerKilled] Victim:     " + lastVictim,"DEBUG", 8);
             WritePluginConsole("[OnPlayerKilled] DamageType: " + lastWeapon,"DEBUG", 8);


             if (lastKiller != "" && lastKiller != lastVictim)
             {

                 if (isprohibitedWeapon(lastUsedWeapon)) PlayerWarn(lastKiller, lastUsedWeapon);

             }


             if (lastKiller == lastVictim)
             {
                 players.Suicide(lastVictim);
             }



             if (lastKiller != lastVictim)
             {
                 if (lastKiller != "") players.Kill(lastKiller);
                 if (lastVictim != "") players.Death(lastVictim);
             }




         }
         catch (Exception ex)
         {
             WritePluginConsole("^1^bOnPlayerKilled returs an Error: ^0^n" + ex.ToString(), "ERROR", 4);
         }
     }

 }

private bool isprohibitedWeapon(string weapon)
{
    try
    {
        WritePluginConsole("[isprohibitedWeapon]", "DEBUG", 6);
        WritePluginConsole("[isprohibitedWeapon] Detected GameType: " + game_version, "DEBUG", 8);
        if (serverMode == "private")
        {
            WritePluginConsole("[isprohibitedWeapon] is ^1^bFALSE^0^n  PRIVATE MODE DISABLES ALL WEAPON RULES", "DEBUG", 8);
            return false;
        }



        if (serverMode == "flagrun")  // FLAGRUN MODE ALSO KEIN KILL ERLAUBT
        {
            WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  FLAGRUN MODE!", "DEBUG", 8);
            isProhibitedWeapon_Result = "flagrun";
            return true;
        }


        if (serverMode == "knife" && !(weapon == "Melee" || weapon == "Knife" || weapon == "Knife_RazorBlade"))  // KNIFE ONLY MODE
        {
            WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  KNIFE ONLY MODE!", "DEBUG", 8);
            isProhibitedWeapon_Result = "knife";
            return true;
        }


        if (serverMode == "pistol")  // PISTOL ONLY MODE
        {
            WritePluginConsole("[isprohibitedWeapon] Check Weapon: " + weapon, "DEBUG", 8);
            List<string> tmp_pistols = new List<string>();

    

            foreach (KeyValuePair<string,enumBoolYesNo> pistol in Allow_Handguns)
            {
                if (pistol.Value == enumBoolYesNo.Yes) tmp_pistols.Add(pistol.Key);

            }

            tmp_pistols.Add("Medkit"); // to fix a BUG with kill weapon Medkit
            tmp_pistols.Add("Death");  // to fix the Problem with exploding tonns



            if (!tmp_pistols.Contains(weapon))
            {

                WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  PROHIBITED PISTOL! " + weapon, "DEBUG", 8);
                isProhibitedWeapon_Result = "pistol";
                return true;

            }
            else
            {
                return false;
                
            }
            
        }


        if (serverMode == "shotgun")  // SHOTGUN ONLY MODE
        {
            WritePluginConsole("[isprohibitedWeapon] Check Weapon: " + weapon, "DEBUG", 8);
            List<string> tmp_Weapons = new List<string>();



            foreach (KeyValuePair<string, enumBoolYesNo> tmpWeapon in Allow_Shotguns)
            {
                if (tmpWeapon.Value == enumBoolYesNo.Yes) tmp_Weapons.Add(tmpWeapon.Key);

            }

            tmp_Weapons.Add("Medkit"); // to fix a BUG with kill weapon Medkit
            tmp_Weapons.Add("Death");  // to fix the Problem with exploding tonns


            if (!tmp_Weapons.Contains(weapon))
            {

                WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  PROHIBITED SHOTGUN! " + weapon, "DEBUG", 8);
                isProhibitedWeapon_Result = "shotgun";
                return true;

            }
            else
            {
                return false;

            }

        }

        if (serverMode == "boltaction")  // BOLTACTION ONLY MODE
        {
            WritePluginConsole("[isprohibitedWeapon] Check Weapon: " + weapon, "DEBUG", 8);
            List<string> tmp_Weapons = new List<string>();



            foreach (KeyValuePair<string, enumBoolYesNo> tmpWeapon in Allow_Boltaction)
            {
                if (tmpWeapon.Value == enumBoolYesNo.Yes) tmp_Weapons.Add(tmpWeapon.Key);

            }

            tmp_Weapons.Add("Medkit"); // to fix a BUG with kill weapon Medkit
            tmp_Weapons.Add("Death");  // to fix the Problem with exploding tonns


            if (!tmp_Weapons.Contains(weapon))
            {

                WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  PROHIBITED BOLT ACTION SNIPER RIFLE! " + weapon, "DEBUG", 8);
                isProhibitedWeapon_Result = "boltaction";
                return true;

            }
            else
            {
                return false;

            }

        }

        if (serverMode == "autosniper")  // AUTOSNIPER ONLY MODE
        {
            WritePluginConsole("[isprohibitedWeapon] Check Weapon: " + weapon, "DEBUG", 8);
            List<string> tmp_Weapons = new List<string>();



            foreach (KeyValuePair<string, enumBoolYesNo> tmpWeapon in Allow_Autosniper)
            {
                if (tmpWeapon.Value == enumBoolYesNo.Yes) tmp_Weapons.Add(tmpWeapon.Key);

            }

            tmp_Weapons.Add("Medkit"); // to fix a BUG with kill weapon Medkit
            tmp_Weapons.Add("Death");  // to fix the Problem with exploding tonns


            if (!tmp_Weapons.Contains(weapon))
            {

                WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  PROHIBITED  AUTOSNIPER RIFLE! " + weapon, "DEBUG", 8);
                isProhibitedWeapon_Result = "autosniper";
                return true;

            }
            else
            {
                return false;

            }

        }

    


        //MAP LIST PROHIBITED WEAPONS - NEW
        if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes)
        {
            if (MapProhibitedWeapons.ContainsKey(ToFriendlyMapName(currentMapFileName)))
            {
                if (MapProhibitedWeapons[ToFriendlyMapName(currentMapFileName)].Contains(weapon))
                {

                    WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  Map prohibited Weapon match  Current Map: " + ToFriendlyMapName(currentMapFileName) + " Current Weapon: " + weapon, "DEBUG", 8);
                    isProhibitedWeapon_Result = "maplist";
                    return true;

                }
            }
        }



        //MODE LIST PROHIBITED WEAPONS
        if (map_prohibitedWeapons_enable == enumBoolYesNo.Yes)
        {
            if (ModeProhibitedWeapons.ContainsKey(ToFriendlyModeName(currentGamemode)))
            {
                if (ModeProhibitedWeapons[ToFriendlyModeName(currentGamemode)].Contains(weapon))
                {

                    WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  Mode prohibited Weapon match  Current Mode: " + ToFriendlyModeName(currentGamemode) + " Current Weapon: " + weapon, "DEBUG", 8);
                    isProhibitedWeapon_Result = "playlist";
                    return true;

                }
            }
        }



        //GENERAL PROHIBITED WEAPONS
        if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes && g_prohibitedWeapons.Contains(weapon))   // GENERELL VERBOTENE WAFFEN
        {
            WritePluginConsole("[isprohibitedWeapon] is ^1^bTRUE^0^n  General prohibited Weapon match", "DEBUG", 8);
            isProhibitedWeapon_Result = "generel";
            return true;
        }





        
    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^b[isprohibitedWeapon] returs an Error: ^0^n" + ex.ToString(), "ERROR", 4);
        WritePluginConsole("^1^b[isprohibitedWeapon] Current Map" + currentMapFileName + "Current Weapon: " + weapon, "DEBUG", 8);
    }
    WritePluginConsole("[isprohibitedWeapon] is ^1^bFALSE^0^n Current Map: " + ToFriendlyMapName(currentMapFileName) + " (" + currentMapFileName + ") Current Mode: " + ToFriendlyModeName(currentGamemode) + " (" + currentGamemode + ") Current Weapon: " + weapon, "DEBUG", 8);
    return false;
}
    
private void PlayerWarn(string name,string weapon)
{





    if (IsAdmin(name) && Prevent_Admins_Warn == enumBoolYesNo.Yes) return; // Routine verlassen wenn Admins nicht gewarnt werden sollen
    if (isInWhitelist(name) && Prevent_WlistPlayers_Warn == enumBoolYesNo.Yes) return; // Routine verlassen wenn Whitelist Players nicht gewarnt werden sollen
    
    try
    {
        int yell_Time = 10;  // In user Settings aufnehmen
        players.Warn(name);
        int warns = players.Warns(name);


        if (isProhibitedWeapon_Result == "generel" || isProhibitedWeapon_Result == "maplist" || isProhibitedWeapon_Result == "playlist")
                {
                    KillPlayer(name);
                    WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + g_max_Warns.ToString(), "KILL", 2);
                    if (warns < g_max_Warns)
                    {
                        SendGlobalMessage(msg_warnBanner);
                        SendGlobalMessage(R(msg_prohibitedWeapon));
                        SendPlayerYellV(name, (R(msg_prohibitedWeapon)), yell_Time);
                        SendGlobalMessage(msg_warnBanner);
                    }


                    if (warns == g_max_Warns) // Maximale Warnungen erreicht
                    {
                        SendGlobalMessage(msg_warnBanner);
                        SendGlobalMessage(R(msg_prohibitedWeapon));
                        SendGlobalMessage(R(msg_prohibitedWeaponLastWarn));
                        SendPlayerYellV(name, (R(msg_prohibitedWeapon + " " + msg_prohibitedWeaponLastWarn)), yell_Time);
                        SendGlobalMessage(msg_warnBanner);
                    }

                    if (warns > g_max_Warns) // Maximale Warnungen erreicht
                    {
                        if (!isInWhitelist(name)) g_Action(name);
                        SendGlobalMessage(R(msg_prohibitedWeaponKick));

                    }

                }
            



        // KNIFE ONLY MODE
        if (isProhibitedWeapon_Result == "knife")   
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + kom_max_Warns.ToString(), "KILL", 2);
            if (warns < kom_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_KnifeWarn));
                SendPlayerYellV(name, (R(msg_KnifeWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }


            if (warns == kom_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_KnifeWarn));
                SendGlobalMessage(R(msg_KnifeLastWarn));
                SendPlayerYellV(name, (R(msg_KnifeWarn + " " + msg_KnifeLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > kom_max_Warns) // Maximale Warnungen ??schritten
            {
                if (!isInWhitelist(name)) kom_Action(name);
                SendGlobalMessage(R(msg_KnifeKick));

            }

        }


        // PISTOL ONLY MODE
        if (isProhibitedWeapon_Result == "pistol") 
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + pom_max_Warns.ToString(), "KILL", 2);
            if (warns < pom_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_PistolWarn));
                SendPlayerYellV(name, (R(msg_PistolWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }


            if (warns == pom_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_PistolWarn));
                SendGlobalMessage(R(msg_PistolLastWarn));
                SendPlayerYellV(name, (R(msg_PistolWarn + " " + msg_PistolLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > pom_max_Warns) // Maximale Warnungen ??schritten
            {
                if (!isInWhitelist(name)) pom_Action(name);
                SendGlobalMessage(R(msg_PistolKick));

            }

        }


        // SHOTGUN ONLY MODE
        if (isProhibitedWeapon_Result == "shotgun")
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + sm_max_Warns.ToString(), "KILL", 2);
            if (warns < sm_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_ShotgunWarn));
                SendPlayerYellV(name, (R(msg_ShotgunWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }


            if (warns == sm_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_ShotgunWarn));
                SendGlobalMessage(R(msg_ShotgunLastWarn));
                SendPlayerYellV(name, (R(msg_ShotgunWarn + " " + msg_ShotgunLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > sm_max_Warns) // Maximale Warnungen ??schritten
            {
                if (!isInWhitelist(name)) sm_Action(name);
                SendGlobalMessage(R(msg_ShotgunKick));

            }

        }

        // BOLTACTION ONLY MODE
        if (isProhibitedWeapon_Result == "boltaction")
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + sm_max_Warns.ToString(), "KILL", 2);
            if (warns < bam_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_BoltActionWarn));
                SendPlayerYellV(name, (R(msg_BoltActionWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }


            if (warns == bam_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_BoltActionWarn));
                SendGlobalMessage(R(msg_BoltActionLastWarn));
                SendPlayerYellV(name, (R(msg_BoltActionWarn + " " + msg_BoltActionLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > sm_max_Warns) // Maximale Warnungen ??schritten
            {
                if (!isInWhitelist(name)) bam_Action(name);
                SendGlobalMessage(R(msg_BoltActionKick));

            }

        }

        // AUTOSNIPER ONLY MODE
        if (isProhibitedWeapon_Result == "autosniper")
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + sm_max_Warns.ToString(), "KILL", 2);
            if (warns < dmr_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_DmrWarn));
                SendPlayerYellV(name, (R(msg_DmrWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }


            if (warns == sm_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_DmrWarn));
                SendGlobalMessage(R(msg_DmrLastWarn));
                SendPlayerYellV(name, (R(msg_DmrWarn + " " + msg_DmrLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > sm_max_Warns) // Maximale Warnungen ??schritten
            {
                if (!isInWhitelist(name)) dmr_Action(name);
                SendGlobalMessage(R(msg_DmrKick));

            }

        }               






        if (isProhibitedWeapon_Result == "flagrun")// && !isInWhitelist(name))
        {
            KillPlayer(name);
            WritePluginConsole("^7WARNED:^1^b " + lastKiller + "^5^n for using ^1^b[ " + weapon + " ]^5^n WARN ^1^b" + warns.ToString() + "^5^n of ^1^b" + fm_max_Warns.ToString(), "KILL", 2);
            if (warns < fm_max_Warns)
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_FlagrunWarn));
                SendPlayerYellV(name, (R(msg_FlagrunWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }



            if (warns == fm_max_Warns) // Maximale Warnungen erreicht
            {
                SendGlobalMessage(msg_warnBanner);
                SendGlobalMessage(R(msg_FlagrunWarn));
                SendGlobalMessage(R(msg_FlagrunLastWarn));
                SendPlayerYellV(name, (R(msg_FlagrunWarn + " " + msg_FlagrunLastWarn)), yell_Time);
                SendGlobalMessage(msg_warnBanner);
            }

            if (warns > fm_max_Warns) // Maximale Warnungen erreicht
            {
                if (!isInWhitelist(name)) fm_Action(name);
                SendGlobalMessage(R(msg_FlagrunKick));

            }

        }




        
        




  
    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^bWarnPlayer returns an Error: ^0^n" + ex.ToString(), "ERROR", 9);
    }

}
 
public override void OnPlayerSpawned(string soldierName, Inventory spawnedInventory) {

// if (serverMode == "private" && !isInWhitelist(soldierName)) kickPlayer(soldierName, msg_pmKick);  // Kick player if not in Whitelist when PRIVATE MODE IS ACTIVE

players.Add(soldierName);
WritePluginConsole("Player spawn detected. Playername = " + soldierName, "DEBUG",8);

if (!spawnedPlayer.Contains(soldierName))
{
    spawnedPlayer.Add(soldierName);
    if (rules_firstjoin == enumBoolYesNo.Yes) ShowRules(soldierName, true); // Show firstspan server rules

}



 }

public override void OnGlobalChat(string speaker, string message) {
OnAnyChat(speaker, message);

 }

public override void OnTeamChat(string speaker, string message, int teamId) {
OnAnyChat(speaker, message);
 }

public override void OnSquadChat(string speaker, string message, int teamId, int squadId) {
OnAnyChat(speaker, message);
 }

public override void OnRoundOverPlayers(List<CPlayerInfo> players) { }

public override void OnRoundOverTeamScores(List<TeamScore> teamScores) { }

public override void OnRoundOver(int winningTeamId) {
players.ResetData();



if (IsSwitchDefined() && plugin_enabled) SwitchServerMode(next_serverMode);

}

public override void OnLoadingLevel(string mapFileName, int roundsPlayed, int roundsTotal) 
{
    WritePluginConsole("[OnLoadingLevel] " + mapFileName, "DEBUG",6);
}

public override void OnLevelStarted()
 {
     WritePluginConsole("[OnLevelStarted]","DEBUG", 6);
 }

public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) // BF3
{
    WritePluginConsole("^1^b[OnLevelLoaded]", "DEBUG", 6);
    WritePluginConsole("^4^bStarted next round: ^2^n " + mapFileName + "^5 " + Gamemode + " ^0 Round " + roundsPlayed.ToString() + " of " + roundsTotal.ToString(), "INFO", 8);
    WritePluginConsole("^4^bStarted next round: ^2^n " + ToFriendlyMapName(mapFileName) + "^5 " + Gamemode + " ^0 Round " + roundsPlayed.ToString() + " of " + roundsTotal.ToString(), "INFO", 6);
    WritePluginConsole("[OnLevelLoaded] " + mapFileName,"DEBUG", 8);
    currentMapFileName = mapFileName;
    currentGamemode = Gamemode;
    currentRound = roundsPlayed;
    totalRounds = roundsTotal;
    spawnedPlayer.Clear();
    if (serverMode == "private" && autoKickAll == enumBoolYesNo.Yes)
    {
        WritePluginConsole("Auto Kick All is enabled... Kicking all not whitelisted players...", "INFO", 2);
        {
            KickAll();
        }

    }
    
    


}



} // end ExtraServerFuncs


public class PlayerDB
{
    private volatile ExtraServerFuncs plugin = new ExtraServerFuncs();
    private volatile BattlelogClient blClient = new BattlelogClient();
    private volatile CSV_Database csv_db = new CSV_Database();
    private volatile List<string> PlayerQ = new List<string>();
    private volatile List<string> Players = new List<string>();
    private volatile Dictionary<String, String> Player_Tag = new Dictionary<string, string>();
    private volatile Dictionary<String, int> Player_Kills = new Dictionary<string, int>();
    private volatile Dictionary<String, int> Player_Death = new Dictionary<string, int>();
    private volatile Dictionary<String, int> Player_Warns = new Dictionary<string, int>();
    private volatile Dictionary<String, int> Player_Suicides = new Dictionary<string, int>();

    
public void Add(string name)
  {
      if (!PlayerQ.Contains(name) && !Players.Contains(name))
      {
          PlayerQ.Add(name);
          GetPlayerData();
      }
  }

public int Count()
{
    return Players.Count;
}   
    

public void Kill(string name)
{
    int k = Player_Kills[name];
    k = k + 1;
    Player_Kills[name] = k;
}

public void Death(string name)
{
    int d = Player_Death[name];
    d = d + 1;
    Player_Death[name]= d;
}

public void Suicide(string name)
{
    int s = Player_Suicides[name];
    s = s + 1;
    Player_Suicides[name]= s;
}

public void Warn(string name)
{
    int w = Player_Warns[name];
    w = w + 1;
    Player_Warns[name] = w;
}

public int Warns(string name)
{
    int w = Player_Warns[name];
    return w;
}

public List<string> ListPlayers()
{
    return Players;
}

private void AddPlayer(string name, string tag)
{

    Players.Add(name);
    Player_Tag.Add(name, tag);
    Player_Kills.Add(name, 0);
    Player_Death.Add(name, 0);
    Player_Warns.Add(name, 0);
    Player_Suicides.Add(name, 0);

    if (!csv_db.isInit()) csv_db.Init(@"Plugins\ExtraServerFuncs_PlayerDB.csv");
    if (!csv_db.isPlayerInDatabase(name))
    {
        CSV_PlayerInfo new_player = new CSV_PlayerInfo();
        new_player.PlayerName = name;
        new_player.ClanTag = tag;
        csv_db.AddPlayer(new_player);
    }
    



}

public void ResetData() // Store Data in CSV Database and reset Round Data
{
    foreach (string name in Players)
    {
        CSV_PlayerInfo tmp_player = csv_db.GetPlayerData(name); 
        if (tmp_player.PlayerName == name)
        {
            tmp_player.ClanTag = Player_Tag[name];
            tmp_player.Kills = tmp_player.Kills + Player_Kills[name];
            tmp_player.Death = tmp_player.Death + Player_Death[name];
            tmp_player.Warns = tmp_player.Warns + Player_Warns[name];
            tmp_player.Suicides = tmp_player.Suicides + Player_Suicides[name];
            tmp_player.Endrounds = tmp_player.Endrounds + 1;
            tmp_player.Visits = tmp_player.Visits + 1;
            tmp_player.LastSeen = DateTime.Now;
            csv_db.SetPlayerData(tmp_player);   
            Player_Kills[name] = 0;
            Player_Death[name] = 0;
            Player_Warns[name] = 0;
            Player_Suicides[name] = 0;    

        
        }
    }
    csv_db.SaveToFile();
}

public void Remove(string name) // Remove Player from DB and save him to CSV_Database
{
    CSV_PlayerInfo tmp_player = csv_db.GetPlayerData(name);
    if (tmp_player.PlayerName == name)
    {
        tmp_player.ClanTag = Player_Tag[name];
        tmp_player.Kills = tmp_player.Kills + Player_Kills[name];
        tmp_player.Death = tmp_player.Death + Player_Death[name];
        tmp_player.Warns = tmp_player.Warns + Player_Warns[name];
        tmp_player.Suicides = tmp_player.Suicides + Player_Suicides[name];
        tmp_player.Visits = tmp_player.Visits + 1;
        tmp_player.LastSeen = DateTime.Now;
        csv_db.SetPlayerData(tmp_player);
    }
  
    Players.Remove(name);
    Player_Tag.Remove(name);
    Player_Kills.Remove(name);
    Player_Death.Remove(name);
    Player_Warns.Remove(name);
    Player_Suicides.Remove(name);
}

public string GetPlayerClanTag(string name) // Gebe das Clantag des Spielers aus der lokalen Datenbank zurück
{
    PlayerInfo tmpPlayer = GetPlayerInfo(name);
    if (tmpPlayer.Tag != "")
    {
        return tmpPlayer.Tag;
    }
    else
    {
        return "EMPTY CLAN TAG";
    }
}


public PlayerInfo GetPlayerInfo(string name)
{ 
    PlayerInfo tmpvar = new PlayerInfo();

    if (Players.Contains(name))
    {
  
        tmpvar.Name = name;
        tmpvar.Tag = Player_Tag[name];
        tmpvar.Kills = Player_Kills[name];
        tmpvar.Death = Player_Death[name];
        tmpvar.Warns = Player_Warns[name];
        tmpvar.Suicides = Player_Suicides[name];
        return tmpvar;
    }
  
    tmpvar.Name = "NOT IN LIST";
    tmpvar.Tag = "NOT IN LIST";
    tmpvar.Kills = 0;
    tmpvar.Death = 0;
    tmpvar.Warns = 0;
    tmpvar.Suicides = 0;
    
    
    return tmpvar;
}

private void GetPlayerData()
{
   if (PlayerQ.Count > 0)
   {
        for (int x = 0; PlayerQ.Count > 0; x++)
        {
            
            string tag = blClient.getClanTag(PlayerQ[0]);
            AddPlayer(PlayerQ[0], tag);
            PlayerQ.Remove(PlayerQ[0]);
        }
   }
    
}
}


public class BattlelogClient : ExtraServerFuncs
    {
      private HttpWebRequest req = null;

      WebClient client = null;

      private String fetchWebPage(ref String html_data, String url)
      {
        try
        {
          if (client == null)
            client = new WebClient();

          client.Headers["User-Agent"] =
          "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
          "(compatible; MSIE 6.0; Windows NT 5.1; " +
          ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

          html_data = client.DownloadString(url);
          return html_data;
        }
        catch (WebException e)
        {
          if (e.Status.Equals(WebExceptionStatus.Timeout))
            throw new Exception("HTTP request timed-out");
          else
            throw;
        }

        return html_data;
      }

      public String getClanTag(String player)
      {
        try
        {
          /* First fetch the player's main page to get the persona id */
          String result = "";
          fetchWebPage(ref result, "http://battlelog.battlefield.com/" + game_version + "/user/" + player);

          String tag = extractClanTag(result, player);

          return tag;
        }
        catch
        {
          //Handle exceptions here however you want
        }

        return null;
      }

      public String extractClanTag(String result, String player)
      {
        /* Extract the player tag */
        Match tag = Regex.Match(result, @"\[\s*([a-zA-Z0-9]+)\s*\]\s*" + player, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (tag.Success)
          return tag.Groups[1].Value;

        return String.Empty;
      }
    }

class TextDatei
{

        
    ///<summary>
    /// Liefert den Inhalt der Datei zur??
    ///</summary>
    ///<param name="sFilename">Dateipfad</param>
    public string ReadFile(String sFilename)
    {
        string sContent = "";

        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            sContent = myFile.ReadToEnd();
            myFile.Close();
        }
        return sContent;
    }

    ///<summary>
    /// Schreibt den ??gebenen Inhalt in eine Textdatei.
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="sLines">zu schreibender Text</param>
    public void WriteFile(String sFilename, String sLines)
    {
        StreamWriter myFile = new StreamWriter(sFilename);
        myFile.Write(sLines);
        myFile.Close();
    }

    ///<summary>
    /// F??den ??gebenen Text an das Ende einer Textdatei an.
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="sLines">anzuf??der Text</param>
    public void Append(string sFilename, string sLines)
    {
        StreamWriter myFile = new StreamWriter(sFilename, true);
        myFile.Write(sLines);
        myFile.Close();
    }

    ///<summary>
    /// Liefert den Inhalt der ??gebenen Zeilennummer zur??
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="iLine">Zeilennummer</param>
    public string ReadLine(String sFilename, int iLine)
    {
        string sContent = "";
        float fRow = 0;
        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            while (!myFile.EndOfStream && fRow < iLine)
            {
                fRow++;
                sContent = myFile.ReadLine();
            }
            myFile.Close();
            if (fRow < iLine)
                sContent = "";
        }
        return sContent;
    }


    public List<string> ReadLines(String sFilename)
    {
        List<string> sContent = new List<string>();
        
        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            while (!myFile.EndOfStream)
            {
              sContent.Add(myFile.ReadLine());
            }
            myFile.Close();
            
        }
        return sContent;
    }











    /// <summary>
    /// Schreibt den ??gebenen Text in eine definierte Zeile.
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="iLine">Zeilennummer</param>
    ///<param name="sLines">Text f??ie ??gebene Zeile</param>
    ///<param name="bReplace">Text in dieser Zeile ??schreiben (t) oder einf?? (f)</param>
    
    
    public void WriteLine(String sFilename, string sLines)
    {
        string sContent = "";
        string[] delimiterstring = { "\r\n" };

        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            sContent = myFile.ReadToEnd();
            myFile.Close();
        }

        string[] sCols = sContent.Split(delimiterstring, StringSplitOptions.None);
                       
            
        

        sContent = "";
        for (int x = 0; x < sCols.Length - 1; x++)
        {
             sContent += sCols[x] + "\r\n";
                        


            
        }
        sContent += sCols[sCols.Length - 1];
        sContent += sLines + "\r\n";

        StreamWriter mySaveFile = new StreamWriter(sFilename);
        mySaveFile.Write(sContent);
        mySaveFile.Close();
    }

    public void DebugWrite(String sFilename, string sLines)
    {
        string sContent = "";
        string[] delimiterstring = { "\r\n" };

        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            sContent = myFile.ReadToEnd();
            myFile.Close();
        }

        string[] sCols = sContent.Split(delimiterstring, StringSplitOptions.None);




        sContent = "";
        for (int x = 0; x < sCols.Length -1; x++)
        {
            if (sCols.Length < 1000) sContent += sCols[x] + "\r\n";
            if (sCols.Length > 1000)
            {
                if (x < 1)
                {
                }
                else
                {
                    sContent += sCols[x] + "\r\n";
                }


            }
        }
        sContent += sCols[sCols.Length - 1];
        sContent += sLines + "\r\n";
        

        StreamWriter mySaveFile = new StreamWriter(sFilename);
        mySaveFile.Write(sContent);
        mySaveFile.Close();
    }

    
    public void WriteLine(String sFilename, int iLine, string sLines, bool bReplace)
    {
        string sContent = "";
        string[] delimiterstring = { "\r\n" };

        if (File.Exists(sFilename))
        {
            StreamReader myFile = new StreamReader(sFilename, System.Text.Encoding.Default);
            sContent = myFile.ReadToEnd();
            myFile.Close();
        }

        string[] sCols = sContent.Split(delimiterstring, StringSplitOptions.None);

        if (sCols.Length >= iLine)
        {
            if (!bReplace)
                sCols[iLine - 1] = sLines + "\r\n" + sCols[iLine - 1];
            else
                sCols[iLine - 1] = sLines;

            sContent = "";
            for (int x = 0; x < sCols.Length - 1; x++)
            {
                sContent += sCols[x] + "\r\n";
            }
            sContent += sCols[sCols.Length - 1];

        }
        else
        {
            for (int x = 0; x < iLine - sCols.Length; x++)
                sContent += "\r\n";

            sContent += sLines;
        }


        StreamWriter mySaveFile = new StreamWriter(sFilename);
        mySaveFile.Write(sContent);
        mySaveFile.Close();
    }
}

class CSV_Database
{
List<CSV_PlayerInfo> csv_Players;
TextDatei files;
private string CSV_Filename;
private const string csv_header = "Player Name;Clan Tag;Visits;Score;Kills;Death;Suicides;Warns;Kicks;Endrounds;Last Seen;Played Time";
private bool db_fileinit = false;


public bool isInit()
{
    return db_fileinit;
}


public void Init(string Filename)
    {
        CSV_Filename = Filename;
        csv_Players = new List<CSV_PlayerInfo>();
        CSV_PlayerInfo csv_Player;
        files = new TextDatei();


        if (File.Exists(Filename))
        {
            List<string> tmpList = files.ReadLines(Filename);
            int lineCount = 0;
            foreach (string line in tmpList)
            {
                lineCount++;
                string[] row = line.Split(';');
                if (lineCount > 1) // Erste Zeile ??springen, da es sich um den Header handelt
                {
                    csv_Player.PlayerName = row[0];
                    csv_Player.ClanTag = row[1];
                    csv_Player.fileline = lineCount;
                    csv_Player.Visits = Convert.ToInt32(row[2]);
                    csv_Player.Score = Convert.ToInt32(row[3]);
                    csv_Player.Kills = Convert.ToInt32(row[4]);
                    csv_Player.Death = Convert.ToInt32(row[5]);
                    csv_Player.Suicides = Convert.ToInt32(row[6]);
                    csv_Player.Warns = Convert.ToInt32(row[7]);
                    csv_Player.Kicks = Convert.ToInt32(row[8]);
                    csv_Player.Endrounds = Convert.ToInt32(row[9]);
                    csv_Player.LastSeen = Convert.ToDateTime(row[10]);
                    csv_Player.PlayedTime = Convert.ToDateTime(row[11]);

                    csv_Players.Add(csv_Player);
                }
            }
        }
        if (!File.Exists(Filename)) files.WriteLine(Filename, csv_header); // Wenn CSV Datei nicht existiert dann schreibe den Header in die erste Zeile
        db_fileinit = true;    
    }

public CSV_PlayerInfo GetPlayerData(string name) // Lese SpielerDaten aus der Liste
{
    
    for (int pl = 0; pl < csv_Players.Count; pl++)
    {

        if (csv_Players[pl].PlayerName == name) return csv_Players[pl];
    }
    CSV_PlayerInfo empty_value = new CSV_PlayerInfo();
    return empty_value;
}

public void SetPlayerData(CSV_PlayerInfo set_data) // Lese SpielerDaten aus der Liste
{
    
    for (int pl = 0; pl < csv_Players.Count; pl++)
    {
        if (csv_Players[pl].PlayerName == set_data.PlayerName) csv_Players[pl] = set_data;
    }
    
}


public bool isPlayerInDatabase(string name)
{
    for (int pl = 0; pl < csv_Players.Count; pl++)
    {
        if (csv_Players[pl].PlayerName == name) return true;
    }
    return false;
}


public void AddPlayer(string name)
{
    if (!isPlayerInDatabase(name))
    {
        CSV_PlayerInfo new_player = new CSV_PlayerInfo();
        new_player.PlayerName = name;
        csv_Players.Add(new_player);
    }
}

public void AddPlayer(CSV_PlayerInfo new_player)
{
    if (!isPlayerInDatabase(new_player.PlayerName))
    {
        csv_Players.Add(new_player);
    }
}


public void SaveToFile()
{
    /*                           // SCHREIBEN DER CSV DATEI UNTERBINDEN DA ES ZU ABST?ZEN GEF?RT HAT
    string Savestring;
    if (!db_fileinit) return; // Breche Schreibvorgng ab wenn die Datenbank nicht Initialisiert wurde
    foreach (CSV_PlayerInfo player in csv_Players)
    {
        Savestring = player.PlayerName + ";" + player.ClanTag + ";" + Convert.ToString(player.Visits) + ";" + Convert.ToString(player.Score) + ";" + Convert.ToString(player.Kills) + ";" + Convert.ToString(player.Death) + ";" + Convert.ToString(player.Suicides) + ";" + Convert.ToString(player.Warns) + ";" + Convert.ToString(player.Kicks) + ";" + Convert.ToString(player.Endrounds) + ";" + Convert.ToString(player.LastSeen) + ";" + Convert.ToString(player.PlayedTime);

        if (player.fileline == 0) files.WriteLine(CSV_Filename, Savestring); // Spieler ist neu aus dem Server und in der Datei nicht vorhanden. Schreibe neuen Datensatz
        if (player.fileline > 1) files.WriteLine(CSV_Filename, player.fileline, Savestring, true); // Datensatz des Spieler in der CSV Datei ??schreien
    }

    csv_Players.Clear();
    db_fileinit = false;
     */
    return;
}



}


public class GitHubClient
{
    private HttpWebRequest req = null;

    WebClient client = null;

    private String fetchWebPage(ref String html_data, String url)
    {
        try
        {
            if (client == null)
                client = new WebClient();

            client.Headers["User-Agent"] =
            "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
            "(compatible; MSIE 6.0; Windows NT 5.1; " +
            ".NET CLR 1.1.4322; .NET CLR 2.0.50727)";

            html_data = client.DownloadString(url);
            return html_data;
        }
        catch (WebException e)
        {
            if (e.Status.Equals(WebExceptionStatus.Timeout))
                throw new Exception("HTTP request timed-out");
            else
                throw;
        }

        return html_data;
    }

    public String getWebsite()
    {
        try
        {
            /* First fetch the player's main page to get the persona id */
            String result = "";
            fetchWebPage(ref result, "https://api.github.com/repos/GladiusGloriae/ExtraServerFuncs1/releases");


            return result;
        }
        catch (Exception e)
        {
            return e.ToString();
            //Handle exceptions here however you want
        }

        return "ERROR FETCHING WEBSITE";
    }


}


public struct PlayerInfo
{
    public string Name;
    public string Tag;
    public int Kills;
    public int Death;
    public int Warns;
    public int Suicides;
    public DateTime JoinTime;
    public DateTime LeaveTime;
}

public struct CSV_PlayerInfo
{
   public string PlayerName;
   public string ClanTag;
   public int fileline;
   public int Visits;
   public int Score;
   public int Kills;
   public int Death;
   public int Suicides;
   public int Warns;
   public int Kicks;
   public int Endrounds;
   public DateTime LastSeen;
   public DateTime PlayedTime;

}

public struct KillWeaponDetails
{
 public String Name;  // weapon name or reason, like "Suicide"
 public String Detail;  // BF4: ammo or attachment
 public String AttachedTo;  // BF4: main weapon when Name is a secondary attachment, like M320
}


} // end namespace PRoConEventsf

