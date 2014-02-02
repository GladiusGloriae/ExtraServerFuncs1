/* ExtraServerFuncs.cs

by MarkusSR1984 & Koerai3


Roadmap:
=================================

 Arbeit für Rainer:

  - Funktionen sortieren ( Bitte aufpassen das du die Funktionen in der selben Klasse lässt ) !!!!!
  - Docu schreiben
  - Die ServerEvents auf alle Servermodes erweitern. Siehe dazu die "OnServerName" Funktion die ist ein Muster
  - direkt darunter findest du die anderen Funktionen z.B. "OnServerMessage"
  - du findest dir sicher arbeit ;)
  
  
  
 - BUGFIXING
			    
            - pb_pBan überprüfen, da nur ein tban ausgesprochen wird
            - readconfig funktioniert nicht ordnungsgemäß
            - Servermodes stehen nach Aktivierung nicht im Startupmode zur Verfügung
            - Manuelles Update der Angezeigten Vriablen in der Plugin Config
  
 
 - Infantry Only Mode
  
 
 - Hardcore Mode
 
   
 - On Map prohibited Weapons
   -evtl über Dictionary lösen
  
  
-  Registered Commands !!!!!
    
  
 
  
- MapLists
  - Dem user die möglichkeit geben eigene Maplists zu erzeugen
  - Dafür eigene Liste mit den Maplist namen erzeuen
  - Dynamich je Maplistname eine Liste mit den Maps erzeugen
  
 - PlayerDB Class umschreiben
    - Die Speicherung soll in einer Liste erfolgen nicht in vielen
  
 - Der Battlelog Client soll in einem eigenen thread laufen da der abruf der daten einige zeit in anspruch nimmt
 

- Plugin USER SETTINGS
	
	- Sort Current Variables
 

- Show WeaponCode Funktion
   - Variable yes/No hinzufügen
   - Ausgabe Farblich hervorheben
  
  
 
 
- Knife Only Mode Aktivieren
    - WarnRoutine Erweitern
    - Usersetting ( WarnCount, Kick/TBan/PBan/pb_tBan/pb_pBan )
    - Warn Message


- Pistol Only Mode Aktivieren
    - Warnroutine Erweitern
    - Usersettings ( WarnCount, Kick/TBan/PBan, Allowed Pistols )
    - Warn Message


- Hardcorde Mode Schreiben
  
  
 
  
- ServerMeldung hinzufügen die jede Minute anzeigt in Welchem Modus sich der Server befindet
  alle Anzeigen ausser dem Normalen Modus
    - gleich mit einem Task erledigen
 

- SendSwitchMessage() Funktion umschreiben
    diese soll in Zukunft nicht mehr in einem Thread laufen
    sondern über einen Task angesprochen werden, = mehr performanche
 
		
- Weitere Servervariablen in Config aufnehmen
		- Die Auslesefunktion umbauen so das nur auf Aufforderung zum Varupdate ausgelesen wird
		- Server Message
		- Server Slots
        - Commander
        - 

- Auto Spectator Funktion
		- Spectatorports
		- öffentliche Zuschauerports ???
		
 
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

    Dictionary<String, List<string>> MapProhibitedWeapons;

// Threads
Thread delayed_message;
Thread countdown_message;
// Classes

TextDatei files;
PlayerDB players;
// GENERAL VARS    
private volatile bool readconfig;
public volatile bool plugin_enabled;
private enumBoolYesNo pm_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo fm_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo kom_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo pom_isEnabled = enumBoolYesNo.No;
private enumBoolYesNo mWhitelist_isEnabled  = enumBoolYesNo.No;
private enumBoolYesNo showweaponcode = enumBoolYesNo.No;
private List<string> m_ClanWhitelist;
private List<string> m_PlayerWhitelist;
private volatile string startup_mode_def;
private volatile string startup_mode = "none";
private volatile string tmp_mapList;
private volatile string SwitchInitiator;
private volatile string lastcmdspeaker;
private int wait;
private enumBoolYesNo g_prohibitedWeapons_enable = enumBoolYesNo.No;
private List<string> g_prohibitedWeapons;

private int g_ActionTbanTime = 60;
private string g_PlayerAction = "kick";

private int adminMsgCount;
private BattlelogClient blClient; // BL Client try

private PlayerInfo tmpvar1; // ONLY FOR TEST
private Dictionary<string, PlayerInfo> PlayerDB = new Dictionary<string, PlayerInfo>();




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
private volatile string switchnow_cmd = "switchnow";     // SWITCHNOW Command
private volatile string rules_command = "rules";         // !rules Command



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
private string msg_warnBanner =         "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";
private string msg_prohibitedWeapon =   "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_prohibitedWeaponLastWarn = "NEXT TIME %kickban%";
private string msg_prohibitedWeaponKick = "KICKED %Killer for using %Weapon%";
private string msg_prohibitedWeaponKickPlayer = "%kickban%ED you for using %Weapon%";
private string msg_countdown =           "SERVER SWITCH TO %nextServermode% IN";
private string msg_FlagrunWarn =        "%Killer%, DO NOT KILL AGAIN!";
private string msg_FlagrunLastWarn =    "NEXT TIME %kickban%!!!";
private string msg_FlagrunKick = "kicked %Killer% for Kill";
private string msg_fmKick =             "kicked you for kills on a Flagrun Server";
private string msg_KnifeKick = "KICKED %Killer for using %Weapon%";
private string msg_komKick = "kicked you for kills with %Weapon% on KNIFE ONLY MODE";
private string msg_KnifeWarn =          "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_KnifeLastWarn =      "NEXT TIME %kickban%!!!";

private string msg_pomKick = "kicked you for kills with %Weapon% on PISTOL ONLY MODE";
private string msg_PistolKick = "KICKED %Killer for using %Weapon%";
private string msg_PistolWarn =         "%Killer%, DO NOT USE %Weapon% AGAIN!";
private string msg_PistolLastWarn =     "NEXT TIME %kickban%!!!";
private string msg_ActionTypeKick =     "KICK";
private string msg_ActionTypeBan =      "BAN";


// NORMAL MODE VARS
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


// FLAGRUN MODE VARS
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
private List<string> kom_ClanWhitelist;
private List<string> kom_PlayerWhitelist;
private string kom_Servername = "Servername - KNIFE ONLY";
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

private enumBoolYesNo pom_allowPistol_M9 = enumBoolYesNo.Yes;               //M9
private enumBoolYesNo pom_allowPistol_QSZ92 = enumBoolYesNo.Yes;            //QSZ-92
private enumBoolYesNo pom_allowPistol_MP443 = enumBoolYesNo.Yes;            //MP-443
private enumBoolYesNo pom_allowPistol_Shorty = enumBoolYesNo.No;            //SHORTY 12G
private enumBoolYesNo pom_allowPistol_Glock18 = enumBoolYesNo.Yes;          //G18
private enumBoolYesNo pom_allowPistol_FN57 = enumBoolYesNo.Yes;             //FN57
private enumBoolYesNo pom_allowPistol_M1911 = enumBoolYesNo.Yes;            //M1911
private enumBoolYesNo pom_allowPistol_93R = enumBoolYesNo.Yes;              //93R
private enumBoolYesNo pom_allowPistol_CZ75 = enumBoolYesNo.Yes;             //CZ-75
private enumBoolYesNo pom_allowPistol_Taurus44 = enumBoolYesNo.Yes;         //.44 MAGNUM
private enumBoolYesNo pom_allowPistol_HK45C = enumBoolYesNo.Yes;            //COMPACT 45
private enumBoolYesNo pom_allowPistol_P226 = enumBoolYesNo.Yes;             //P226
private enumBoolYesNo pom_allowPistol_MP412Rex = enumBoolYesNo.Yes;         //M412 REX
private enumBoolYesNo pom_allowPistol_Meele = enumBoolYesNo.Yes;            //Knife






public ExtraServerFuncs() {
	fIsEnabled = false;
	fDebugLevel = 2;

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
    pm_MapList.Add("MP_Journey SquadDeathMatch0 2"); // Goldmud
    pm_MapList.Add("MP_Prison SquadDeathMatch0 2");  // Spind
        
    pom_MapList = new List<string>(); 			// PISTOL ONLY MODE MAPLIST
    pom_MapList.Add("MP_Prison SquadDeathMatch0 2");  // Spind

    kom_MapList = new List<string>(); 			// KNIFE ONLY MODE MAPLIST
    kom_MapList.Add("MP_Prison SquadDeathMatch0 2");  // Spind
    
    fm_MapList = new List<string>(); 			// FLAGRUN MODE MAPLIST
    fm_MapList.Add("MP_Flooded ConquestLarge0 2"); // Floodzone
    fm_MapList.Add("MP_Journey ConquestLarge0 2"); // Goldmud
    




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


// TRY FOR MAP PROHIBITED WEAPONS
MapProhibitedWeapons = new Dictionary<string, List<string>>();



}

private void PluginCommand(string cmd) // Set Speaker to Server if no Speaker is send. e.G. command from ProConPlugin Config
{
PluginCommand("Server", cmd);
}

private void PluginCommand(string cmdspeaker, string cmd) // Routine zur Bereitstellung von Plugin und Chat Commands in Procon
{
    
    if (plugin_enabled)
    {


        lastcmdspeaker = cmdspeaker;

        if (cmd == rules_command)
        {
            WritePluginConsole(cmdspeaker + " requested the Rules", "Info", 0);
            ShowRules(cmdspeaker);
            return;
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



        
        if (cmd == "try")
        {
            
            WritePluginConsole("Start test...Class PlayerDB", "TRY", 0);
            WritePluginConsole("Write a csv file", "TRY", 0);


            List<string> item;
                
            item = new List<string>();
            item.Add("MP_Journy_Weapon1");
            item.Add("MP_Journy_Weapon2");
            item.Add("MP_Journy_Weapon3");
            MapProhibitedWeapons.Add("MP_Journy", item);

            item = new List<string>();
            item.Add("MP_Prison_Weapon1");
            item.Add("MP_Prison_Weapon2");
            item.Add("MP_Prison_Weapon3");
            MapProhibitedWeapons.Add("MP_Prison", item);


            foreach (KeyValuePair<string, List<string>> entry in MapProhibitedWeapons)
            {
                WritePluginConsole("STRING/LIST: Key = " + entry.Key, "TRY", 0);
                foreach (string V in entry.Value)
                {
                    WritePluginConsole("STRING/LIST: Value = " + V, "TRY", 0);
                }
            }
                
                //lstReturn.Add(new CPluginVariable("10. On Map prohibited Weapons|" + entry.Key, typeof(string[]), entry.Value));
                
            


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



        WritePluginConsole("Unknown Command", "Error", 0);
        return;

    }
	
}


#region TESTROUTINEN

public void WriteData(string path, string s)
{
    try
    {
        //if (File.Exists(path))
        //    File.Delete(path);

        using (FileStream fs = File.Open(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            Byte[] info = new UTF8Encoding(true).GetBytes(s);
            fs.Write(info, 0, info.Length);
        }
    }
    catch (Exception ex)
    {
        ConsoleError("unable to dump information to file");
        ConsoleException("" + ex.GetType() + ": " + ex.Message);
    }
}


#endregion


public string FWeaponName(String killWeapon)  // Returns Returns a String with the Used Weapon
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
            DebugWrite("Warning: unrecognized weapon code: " + killWeapon, 5);
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
            DebugWrite("Warning: unrecognized weapon code: " + killWeapon, 5);
        }
    }
    return r;
}

private void ReadServerConfig()
{
    WritePluginConsole("Call ReadServerConfig()", "DEBUG", 10);
    readconfig = true;
    WritePluginConsole(R("Read ServerVars and save them to %currServermode%"), "Info", 0);
       
    
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
		
private void ShowRules()
		{
		ShowRules("all");
		}
	
private void ShowRules(string playerName)
		{
			WritePluginConsole("Read Servermode for show rules: " + serverMode, "Info", 6);
			if (serverMode == "normal")  ShowRules(playerName, nm_Rules);
			if (serverMode == "private") ShowRules(playerName, pm_Rules);
            if (serverMode == "knife") ShowRules(playerName, kom_Rules);
            if (serverMode == "pistol") ShowRules(playerName, pom_Rules);
            if (serverMode == "flagrun") ShowRules(playerName, fm_Rules);
            
		}
		
private void ShowRules(string playerName, List<string> RulesList)
		{
			foreach (string rule in RulesList)
			{
				WritePluginConsole("Sending Rule to " + playerName + " rule: " + rule, "Info", 6);
				if (playerName == "all") SendGlobalMessage(rule);
				if (playerName != "all") SendPlayerMessage(playerName, rule);
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
        message = message + " (tBan " + (minutes).ToString() + " minutes) by [AUTOADMIN]";
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", name, "seconds", (minutes * 60).ToString(), message);
        this.ExecuteCommand("procon.protected.send", "banList.save");
        kickPlayer(name, message);
        return true;
    }

public bool pbanPlayer(string name, string message)
    {
        message = message + " (Permanent Ban) by [AUTOADMIN]";
        this.ExecuteCommand("procon.protected.send", "banList.add", "name", name,"perm", message);
        this.ExecuteCommand("procon.protected.send", "banList.save");
        kickPlayer(name, message);
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
	WritePluginConsole("Read map from list: "+i+"    " + MapList[i], "Info", 2);
	WritePluginConsole("Splitted result 1 : "+ splitMapList[0], "Info", 10);
	WritePluginConsole("Splitted result 2 : "+ splitMapList[1], "Info", 10);
	WritePluginConsole("Splitted result 3 : "+ splitMapList[2], "Info", 10);
	
	this.ExecuteCommand("procon.protected.send", "mapList.add", splitMapList[0], splitMapList[1], splitMapList[2]);
	this.ExecuteCommand("procon.protected.send", "mapList.save");

    this.ExecuteCommand("procon.protected.tasks.add", "Switch", "2", "1", "1", "procon.protected.send", "mapList.getMapIndices");
    this.ExecuteCommand("procon.protected.tasks.add", "Switch", "3", "1", "1", "procon.protected.send", "mapList.setNextMapIndex", "0");


	}

	
return;
}

public bool IsSwitchDefined()  // Is defined a Servermode Switch ?
{
WritePluginConsole("Called IsSwitchDefined(): serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
if (next_serverMode != serverMode) return true;
return false;
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






    if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes && g_prohibitedWeapons.Contains(lastUsedWeapon))
    {
        if (g_PlayerAction == "kick" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeKick);
        if (g_PlayerAction == "tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pb_tban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);
        if (g_PlayerAction == "pb_pban" && text.Contains("%kickban%")) text = text.Replace("%kickban%", msg_ActionTypeBan);

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
   


    
    
//        private string msg_komKick = "kicked you for kills with %Weapon% on KNIFE ONLY MODE";
//private string msg_KnifeWarn =          "%Killer%, DO NOT USE %Weapon% AGAIN!";
//private string msg_KnifeLastWarn =      "NEXT TIME %kickban%!!!";


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





public void PreSwitchServerMode(string MarkNewServerMode) // Define a Server Mode Switch
{
    WritePluginConsole("Called PreSwitchServerMode: serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
    next_serverMode = MarkNewServerMode;
    SendSwitchMessage();
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
        if (!plugin_enabled) return;
        WritePluginConsole("Called StartSwitchCountdown()", "INFO", 10);
        string countdown_message = (R(msg_countdown));
        //string countdown_message = (msg_countdown);
        int counter = countdown_time;
        int timer = 0;
        



        while (counter >= 0)
        {


            string msg_Count = countdown_message + " " + counter.ToString() + " SECONDS";

            //procon.protected.tasks.add <int: delay> <int: interval> <int: repeat> [[vars: commandwords] ...]

            this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncsYell", timer.ToString(), "1", "1", "procon.protected.send", "admin.yell", msg_Count, "2", "all");
           // this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncsSay", timer.ToString(), "1", "1", "procon.protected.send", "admin.say", msg_Count, "1", "all");
            this.ExecuteCommand("procon.protected.tasks.add", "ExtraServerFuncsCon", timer.ToString(), "1", "1", "procon.protected.pluginconsole.write", msg_Count, "2", "all");



            timer++;
            counter--;
        }

        this.ExecuteCommand("procon.protected.tasks.add", "Switch", timer.ToString() , "1", "1", "procon.protected.plugins.call", "ExtraServerFuncs", "SwitchNow");
        this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 5).ToString(), "1", "1", "procon.protected.send", "mapList.getMapIndices");
        this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 6).ToString(), "6", "1", "procon.protected.send", "mapList.setNextMapIndex", "0");
        this.ExecuteCommand("procon.protected.tasks.add", "Switch", (timer + 7).ToString(), "7", "1", "procon.protected.send", "mapList.runNextRound");
        
        
        



    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^bStartSwitchCountdown returs an Error: ^0^n" + ex.ToString(), "ERROR", 9);
    }
        //}
    
    return;
    
    


}




public void SwitchServerMode(string newServerMode)  // Switch the current Server Mode
{
    WritePluginConsole("Called Switch Server Mode", "Info", 10);
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
        nm_PlayerSpawnCount
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
        pm_PlayerSpawnCount

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
        fm_PlayerSpawnCount

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
        pom_PlayerSpawnCount

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
        kom_PlayerSpawnCount

    );

}




}

public void WriteServerConfig(string newName, string Description, string Message, List<string> NewMaplist ,enumBoolYesNo VehicleSpawnAllowed,int VehicleSpawnTime, int PlayerSpawnTime)  // Write the Config to the Server
{
    Thread thread_writeserverconfig = new Thread(new ThreadStart(delegate()
        {
            this.WritePluginConsole("Called WriteServerConfig(): serverMode= " + serverMode + " next_serverMode = " + next_serverMode, "Warn", 10);
            if (autoconfig == enumBoolYesNo.Yes) tmp_autoconfig = "Yes";
            if (autoconfig == enumBoolYesNo.No) tmp_autoconfig = "No";
            this.SetPluginVariable("Autoconfig", "No" );
            Thread.Sleep(1000);
            this.ServerCommand("vars.serverName", newName);                 // SET SERVER NAME
            this.ServerCommand("vars.serverDescription", Description);      // SET SERVER DESCRIPTION
            this.ServerCommand("vars.serverMessage", Message);              // SET SERVER MESSAGE !!!!  NOT SUPORTED YET !!!!!
            this.ServerCommand("vars.vehicleSpawnAllowed", enumboolToStringTrueFalse(VehicleSpawnAllowed));              // SET SERVER MESSAGE !!!!  NOT SUPORTED YET !!!!!
            this.ServerCommand("vars.vehicleSpawnDelay", VehicleSpawnTime.ToString());
            this.ServerCommand("vars.playerRespawnTime", PlayerSpawnTime.ToString());
            
    

    



            
            Thread.Sleep(1000);
            this.WriteMapList(NewMaplist);
            Thread.Sleep(1000);
            this.SetPluginVariable("Autoconfig", tmp_autoconfig );
        }));

thread_writeserverconfig.Start();
}

private bool isInWhitelist(string wlPlayer)   // Erweitern!!! Die Gamemodes müssen eingefügt werden
    {
		WritePluginConsole("Check if " + wlPlayer + " is in Whitelist", "Info", 2);
        /*
        currentPrivileges = GetAccountPrivileges(wlPlayer);

        Use ProCon Account as wlist Player
        if (currentPrivileges != null)
        {
            if (currentPrivileges.CanLogin)
            return true;
        }
	  
		
         

         
         
         
         
         */


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
				this.blClient = new BattlelogClient();
				WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " Check General Clan Whitelist...", "Info", 5);
				if (m_ClanWhitelist.Contains(this.blClient.getClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in General Clan Whitelist
				{
					WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " is in General Clan Whitelist", "Info", 2);
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
				this.blClient = new BattlelogClient();
				WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " Check PRIVATE MODE Clan Whitelist...", "Info", 5);
				if (pm_ClanWhitelist.Contains(this.blClient.getClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
				{
					WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " is in PRIVATE MODE Clan Whitelist", "Info", 2);
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
                this.blClient = new BattlelogClient();
                WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " Check FLAGRUN MODE Clan Whitelist...", "Info", 5);
                if (fm_ClanWhitelist.Contains(this.blClient.getClanTag(wlPlayer)))	// Get Player Clantag from Battlelog and check if it is in Clan Whitelist
                {
                    WritePluginConsole(wlPlayer + " has Clantag: " + this.blClient.getClanTag(wlPlayer) + " is in FLAGRUN MODE Clan Whitelist", "Info", 2);
                    return true;
                }
            }
        }








	WritePluginConsole(wlPlayer + " is not in Whitelist", "Info", 2);
	return false;
    }

public bool ListsEqual(List<MaplistEntry> a, List<MaplistEntry> b) // Vergleich und Schreiben der Mapliste
        {

	



		WritePluginConsole("Current MAPLIST ~~~~~~~~~~~~~~~", "Info", 5);


			tmp_mapList = "";
			
			for (int i = 0; i < a.Count; i++)
            {
                    
                    WritePluginConsole(a[i].MapFileName + " " + a[i].Gamemode + " " + a[i].Rounds, "Info", 5);
					tmp_mapList = tmp_mapList + a[i].MapFileName + " " + a[i].Gamemode + " " + a[i].Rounds;
					if ( i < a.Count - 1) tmp_mapList = tmp_mapList + "|";
					
					
			}

			if (autoconfig == enumBoolYesNo.Yes || readconfig) 
			{
				if (serverMode == "normal") SetPluginVariable("NM_MapList", tmp_mapList );  // SAVE MAPLIST TO NORMAL MODE
				if (serverMode == "private") SetPluginVariable("PM_MapList", tmp_mapList );  // SAVE MAPLIST TO PRIVATE MODE
                if (serverMode == "flagrun") SetPluginVariable("FM_MapList", tmp_mapList);  // SAVE MAPLIST TO FLAGRUN MODE
                if (serverMode == "knife") SetPluginVariable("KOM_MapList", tmp_mapList);  // SAVE MAPLIST TO KNIFE ONLY MODE
                if (serverMode == "pistol") SetPluginVariable("POM_MapList", tmp_mapList);  // SAVE MAPLIST TO PISTOL ONLY MODE
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
            if (tag == "Error")
            {
                tag = "^8" + tag;
            }
            else if (tag == "Work")
            {
                tag = "^4" + tag;
            }
            else
            {
                tag = "^5" + tag;
            }
            string line = "^b[" + this.GetPluginName() + "] " + tag + ": ^0^n" + message;
            
            if (this.fDebugLevel >= level)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }

           
            
        }

public enum MessageType { Warning, Error, Exception, Normal };

public String FormatMessage(String msg, MessageType type) {
	String prefix = "[^bExtraServerFuncs!^n] ";

	if (type.Equals(MessageType.Warning))
		prefix += "^1^bWARNING^0^n: ";
	else if (type.Equals(MessageType.Error))
		prefix += "^1^bERROR^0^n: ";
	else if (type.Equals(MessageType.Exception))
		prefix += "^1^bEXCEPTION^0^n: ";

	return prefix + msg;
}

public void LogWrite(String msg)
{
	this.ExecuteCommand("procon.protected.pluginconsole.write", msg);
}

public void ConsoleWrite(string msg, MessageType type)
{
	LogWrite(FormatMessage(msg, type));
}

public void ConsoleWrite(string msg)
{
	ConsoleWrite(msg, MessageType.Normal);
}

public void ConsoleWarn(String msg)
{
	ConsoleWrite(msg, MessageType.Warning);
}

public void ConsoleError(String msg)
{
	ConsoleWrite(msg, MessageType.Error);
}

public void ConsoleException(String msg)
{
	ConsoleWrite(msg, MessageType.Exception);
}

public void DebugWrite(string msg, int level)
{
	if (fDebugLevel >= level) ConsoleWrite(msg, MessageType.Normal);
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
	return "0.0.1.0";
}

public string GetPluginAuthor() {
	return "MarkusSR1984";
}

public string GetPluginWebsite() {
    return "github.com/MarkusSR1984/ExtraServerFuncs.git";
}

public string GetPluginDescription() {
	return @"

<p>....with some help from Koerai3</p>
<p>
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

<blockquote><h4>NORMAL MODE</h4>
The server runs normaly and public<br/>
</blockquote>

<blockquote><h4>PRIVATE MODE</h4>
The server runs in an private mode. Only players you added to the whitelist can join the server. Player who are on your server before u activate this mode do not get kicked automaticly. You can use this mode to train with your clanmates without being distrubt from other players. Or whatever u want.<br/>
</blockquote>

<blockquote><h4>PISTOL ONLY MODE</h4>
Easy set te pistols u want to allow in the plugin settings. and please remember to write some weapons you do not allow in the rules field<br/>
</blockquote>

<blockquote><h4>KNIFE ONLY MODE</h4>
Only Knife is allowed on this servermode<br/>
</blockquote>

<blockquote><h4>FLAGRUN MODE</h4>
On this mode no kills are allowed. You can use this to fill your server in late hours or if u want to play this mode<br/>
</blockquote>


<h2>In Game Commands</h2>
<p>Please notice that i show you the default config here. All commands are editable in the Plugin setting. If you had done this, this commands would not work together. </p>
<blockquote><h4>!normal</h4>
Select NORMAL MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!private</h4>
Select PRIVATE MODE as next Servermode.<br/>
</blockquote>

<blockquote><h4>!pistol</h4>
Select PRIVATE MODE as next Servermode.<br/>
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

<blockquote><h4>!rules</h4>
Show the Servermode specific rules.<br/>
</blockquote>
</p>


<h2>Settings</h2>
<p>coming soon....</p>


<h2>Changelog</h2>

<blockquote><h4>0.0.1.0 (02-02-2014)</h4>
	- ALPHA<br/>
	- Give it out to ALPHA testers<br/>
	- Added Pistol only mode<br/>
</blockquote>

<blockquote><h4>0.0.0.1 (15-NOV-2013)</h4>
	- PRE ALPHA<br/>
	- initial development version<br/>
</blockquote>
";
}
#endregion

public List<CPluginVariable> GetDisplayPluginVariables() // Liste der Anzuzeigenden Plugin variablen
    {     // Optionen zum erstellen der Konfigvariablen / dem Usermenü

        List<CPluginVariable> lstReturn = new List<CPluginVariable>();



        // BASIC SETTINGS ##################################################################################################################



        lstReturn.Add(new CPluginVariable("1.Basic Settings|I have read the Terms of Use YES / NO", typeof(string), thermsofuse));
        


        if (thermsofuse == "YES")
        {



            lstReturn.Add(new CPluginVariable("1.Basic Settings|Private Mode", typeof(enumBoolYesNo), pm_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Flagrun Mode", typeof(enumBoolYesNo), fm_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Knife Only Mode", typeof(enumBoolYesNo), kom_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Pistol Only Mode", typeof(enumBoolYesNo), pom_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Use General Whitelist", typeof(enumBoolYesNo), mWhitelist_isEnabled));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Use General Prohibited Weapons", typeof(enumBoolYesNo), g_prohibitedWeapons_enable));
            if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("1.Basic Settings|General Prohibited Weapons List", typeof(string[]), g_prohibitedWeapons.ToArray()));
                lstReturn.Add(new CPluginVariable("1.Basic Settings|G_Max Player Warns", typeof(int), g_max_Warns));
                lstReturn.Add(new CPluginVariable("1.Basic Settings|G_Player Action", "enum.fm_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", g_PlayerAction));
                if (g_PlayerAction == "tban" || g_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("1.Basic Settings|G_TBan Minutes", typeof(int), g_ActionTbanTime));
            }



            
            startup_mode_def = "enum.startup_mode(none|normal|private|flagrun|knife|pistol)";
            
            if (nm_Servername != "Your Server Name") lstReturn.Add(new CPluginVariable("1.Basic Settings|Startup Mode", startup_mode_def, startup_mode));
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Show Waponcodes", typeof(enumBoolYesNo), showweaponcode));			

            
            
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Plugin Autoconfig", typeof(enumBoolYesNo), autoconfig));			
			if (mWhitelist_isEnabled == enumBoolYesNo.Yes)
			{
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Clan_Whitelist", typeof(string[]), m_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("1.Basic Settings|Player_Whitelist", typeof(string[]), m_PlayerWhitelist.ToArray()));
			}
            lstReturn.Add(new CPluginVariable("1.Basic Settings|Plugin Command", typeof(string), ""));
            // lstReturn.Add(new CPluginVariable("1.Basic Settings|Plugin Command", "enum.plugin_command(...|normal|private|switchnow|readconfig)", ""));
            

            // NORMAL MODE SETTING ##################################################################################################################


            
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Rules", typeof(string[]), nm_Rules.ToArray()));
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Name", typeof(string), nm_Servername));
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Description", typeof(string), nm_Serverdescription));
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Server Message", typeof(string), nm_ServerMessage));
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), nm_VehicleSpawnAllowed));




            if (nm_VehicleSpawnAllowed == enumBoolYesNo.Yes)
            {
                lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Vehicle Spawn Time", typeof(int), nm_VehicleSpawnCount));
            }
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_Player Spawn Time", typeof(int), nm_PlayerSpawnCount));
            lstReturn.Add(new CPluginVariable("2.Normal Mode|NM_MapList", typeof(string[]), nm_MapList.ToArray()));

            
            
            // PRIVATE MODE SETTING ##################################################################################################################    
            
            

            
            
            if (pm_isEnabled == enumBoolYesNo.Yes) // PRIVATE MODE
            {
                
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Rules", typeof(string[]), pm_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Name", typeof(string), pm_Servername));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Description", typeof(string), pm_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Server Message", typeof(string), pm_ServerMessage));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), pm_VehicleSpawnAllowed));
                                
                if (pm_VehicleSpawnAllowed == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Vehicle Spawn Time", typeof(int), pm_VehicleSpawnCount));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_Player Spawn Time", typeof(int), pm_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_MapList", typeof(string[]), pm_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_ClanWhitelist", typeof(string[]), pm_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.1_Private Mode|PM_PlayerWhitelist", typeof(string[]), pm_PlayerWhitelist.ToArray()));

            }

            
            
            
            // FLAGRUN MODE SETTING ##################################################################################################################    





            if (fm_isEnabled == enumBoolYesNo.Yes) // FLAGRUN MODE
            {

                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Rules", typeof(string[]), fm_Rules.ToArray()));

                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Name", typeof(string), fm_Servername));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Description", typeof(string), fm_Serverdescription));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Server Message", typeof(string), fm_ServerMessage));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), fm_VehicleSpawnAllowed));

                 if (fm_VehicleSpawnAllowed == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Vehicle Spawn Time", typeof(int), fm_VehicleSpawnCount));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Player Spawn Time", typeof(int), fm_PlayerSpawnCount));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_MapList", typeof(string[]), fm_MapList.ToArray()));

                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_ClanWhitelist", typeof(string[]), fm_ClanWhitelist.ToArray()));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_PlayerWhitelist", typeof(string[]), fm_PlayerWhitelist.ToArray()));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Max Player Warns", typeof(int), fm_max_Warns));
                 lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_Player Action", "enum.fm_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", fm_PlayerAction));
                 if (fm_PlayerAction == "tban" || fm_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.2_Flagrun Mode|FM_TBan Minutes", typeof(int), fm_ActionTbanTime));

                

                 
            }

            // KNIFE ONLY MODE SETTING ##################################################################################################################    





            if (kom_isEnabled == enumBoolYesNo.Yes) // KNIFE ONLY MODE
            {

                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Rules", typeof(string[]), kom_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Name", typeof(string), kom_Servername));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Description", typeof(string), kom_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Server Message", typeof(string), kom_ServerMessage));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), kom_VehicleSpawnAllowed));

                if (kom_VehicleSpawnAllowed == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Vehicle Spawn Time", typeof(int), kom_VehicleSpawnCount));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Player Spawn Time", typeof(int), kom_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_MapList", typeof(string[]), kom_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_ClanWhitelist", typeof(string[]), kom_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_PlayerWhitelist", typeof(string[]), kom_PlayerWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Max Player Warns", typeof(int), kom_max_Warns));
                lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_Player Action", "enum.kom_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", kom_PlayerAction));
                if (kom_PlayerAction == "tban" || kom_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.3_Knife only Mode|KOM_TBan Minutes", typeof(int), kom_ActionTbanTime));




            }





            // PISTOL ONLY MODE SETTING ##################################################################################################################    





            if (pom_isEnabled == enumBoolYesNo.Yes) // PISTOL ONLY MODE
            {

                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Rules", typeof(string[]), pom_Rules.ToArray()));

                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Name", typeof(string), pom_Servername));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Description", typeof(string), pom_Serverdescription));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Server Message", typeof(string), pom_ServerMessage));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Vehicle Spawn Allowed", typeof(enumBoolYesNo), pom_VehicleSpawnAllowed));

                if (pom_VehicleSpawnAllowed == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Vehicle Spawn Time", typeof(int), pom_VehicleSpawnCount));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Player Spawn Time", typeof(int), pom_PlayerSpawnCount));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_MapList", typeof(string[]), pom_MapList.ToArray()));

                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_ClanWhitelist", typeof(string[]), pom_ClanWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_PlayerWhitelist", typeof(string[]), pom_PlayerWhitelist.ToArray()));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Max Player Warns", typeof(int), pom_max_Warns));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Player Action", "enum.pom_PlayerAction(kick|tban|pban|pb_tban|pb_pban)", pom_PlayerAction));
                if (pom_PlayerAction == "tban" || pom_PlayerAction == "pb_tban") lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_TBan Minutes", typeof(int), pom_ActionTbanTime));
                //PISTOLS8
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M9", typeof(enumBoolYesNo), pom_allowPistol_M9));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow QSZ-92", typeof(enumBoolYesNo), pom_allowPistol_QSZ92));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow MP-443", typeof(enumBoolYesNo), pom_allowPistol_MP443));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow SHORTY 12G", typeof(enumBoolYesNo), pom_allowPistol_Shorty));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow G18", typeof(enumBoolYesNo), pom_allowPistol_Glock18));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow FN57", typeof(enumBoolYesNo), pom_allowPistol_FN57));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M1911", typeof(enumBoolYesNo), pom_allowPistol_M1911));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow 93R", typeof(enumBoolYesNo), pom_allowPistol_93R));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow CZ-75", typeof(enumBoolYesNo), pom_allowPistol_CZ75));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow .44 MAGNUM", typeof(enumBoolYesNo), pom_allowPistol_Taurus44));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow COMPACT 45", typeof(enumBoolYesNo), pom_allowPistol_HK45C));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow P226", typeof(enumBoolYesNo), pom_allowPistol_P226));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow M412 REX", typeof(enumBoolYesNo), pom_allowPistol_MP412Rex));
                lstReturn.Add(new CPluginVariable("3.4_Pistol only Mode|POM_Allow KNIFE", typeof(enumBoolYesNo), pom_allowPistol_Meele));


            }



            // COMMANDS ##################################################################################################################
            lstReturn.Add(new CPluginVariable("4.Plugin Commands|NM_Command Enable", typeof(string), nm_commandEnable));
            if (pm_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|PM_Command Enable", typeof(string), pm_commandEnable));
            if (kom_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|KOM_Command Enable", typeof(string), kom_commandEnable));
            if (pom_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|POM_Command Enable", typeof(string), pom_commandEnable));
            if (fm_isEnabled == enumBoolYesNo.Yes) lstReturn.Add(new CPluginVariable("4.Plugin Commands|FM_Command Enable", typeof(string), fm_commandEnable));
            lstReturn.Add(new CPluginVariable("4.Plugin Commands|Switchnow_Command", typeof(string), switchnow_cmd));


            // MESSAGES ##################################################################################################################
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PlayerInfo", typeof(string), player_message));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_AdminInfo_switchnow", typeof(string), admin_message));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_SwitchNow", typeof(string), msg_switchnow));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PM Player Kick", typeof(string), msg_pmKick));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_NOT Initiator", typeof(string), msg_notInitiator));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_Switch not defined", typeof(string), msg_switchnotdefined));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_NORMAL MODE", typeof(string), msg_normalmode));
            lstReturn.Add(new CPluginVariable("5.Plugin Messages|MSG_PRIVATE MODE", typeof(string), msg_privatemode));
            


            // Debugkonfig
            lstReturn.Add(new CPluginVariable("6.Debug|Debug level", fDebugLevel.GetType(), fDebugLevel));
            
            //Try to get a dynamic list
           
                //foreach (KeyValuePair<string, List<string>> entry in MapProhibitedWeapons)
                //{
                //    string tmpvar = "10. On Map prohibited Weapons|" + entry.Key;
                //    lstReturn.Add(new CPluginVariable(tmpvar, typeof(string[]), (entry.Value).ToArray()));

                //}
            string key = "TestMap";
            string[] value = new string[1] { "Test Value" };
            lstReturn.Add(new CPluginVariable("10. Test|" + key, value.GetType(), value ));
           

		}
		return lstReturn;
	}




public List<CPluginVariable> GetPluginVariables()  // Liste der Plugin Variablen
{
	return GetDisplayPluginVariables();
} 




public void SetPluginVariable(string strVariable, string strValue) {

    DebugWrite("[VARNAME] " + strVariable + " [VALUE] " + strValue, 5);
    UpdateStartupMode(); //Update Valus in Startup mode

    if (Regex.Match(strVariable, @"I have read the Terms of Use YES / NO").Success)
    {               
        thermsofuse = strValue;
    }

    
    
    if (Regex.Match(strVariable, @"Debug level").Success) {
		int tmp = 2;
		int.TryParse(strValue, out tmp);
		fDebugLevel = tmp;
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


    if (Regex.Match(strVariable, @"Show Waponcodes").Success)
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
	
        if (Regex.Match(strVariable, @"Use General Prohibited Weapons").Success)
    {

        if (strValue == "Yes") g_prohibitedWeapons_enable = enumBoolYesNo.Yes;
        if (strValue == "No") g_prohibitedWeapons_enable = enumBoolYesNo.No;

    }


        if (Regex.Match(strVariable, @"General Prohibited Weapons List").Success)
    {
		g_prohibitedWeapons = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

        
    
    if (Regex.Match(strVariable, @"Clan_Whitelist").Success)
    {
		m_ClanWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }
	
	if (strVariable == "Player_Whitelist")
    {
		m_PlayerWhitelist = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    if (Regex.Match(strVariable, @"G_Max Player Warns").Success)
    {
        g_max_Warns = Convert.ToInt32(strValue);
    }

    if (Regex.Match(strVariable, @"G_Player Action").Success)
    {
        g_PlayerAction = strValue;
    }

    if (Regex.Match(strVariable, @"G_TBan Minutes").Success)
    {
        g_ActionTbanTime = Convert.ToInt32(strValue);
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
            ConsoleWrite("Incorrect Value of NM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of NM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        nm_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"NM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            ConsoleWrite("Incorrect Value of NM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of NM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
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
        if (strValue == "Yes") pm_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") pm_isEnabled = enumBoolYesNo.No;
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
            ConsoleWrite("Incorrect Value of PM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of PM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        pm_VehicleSpawnCount = tmpValue;
    } 
    
    if (Regex.Match(strVariable, @"PM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            ConsoleWrite("Incorrect Value of PM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        
        
        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of PM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


       pm_PlayerSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"PM_MapList").Success)
    {
        pm_MapList = new List<string>(CPluginVariable.DecodeStringArray(strValue));
    }

    // FLAGRUN MODE VARIABLEN
    if (Regex.Match(strVariable, @"Flagrun Mode").Success)
    {
        if (strValue == "Yes") fm_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") fm_isEnabled = enumBoolYesNo.No;
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
            ConsoleWrite("Incorrect Value of FM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of FM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        fm_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"FM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            ConsoleWrite("Incorrect Value of PM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of FM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
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
        if (strValue == "Yes") kom_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") kom_isEnabled = enumBoolYesNo.No;
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
            ConsoleWrite("Incorrect Value of KOM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of KOM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        kom_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"KOM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            ConsoleWrite("Incorrect Value of PM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of KOM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
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
        if (strValue == "Yes") pom_isEnabled = enumBoolYesNo.Yes;
        if (strValue == "No") pom_isEnabled = enumBoolYesNo.No;
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
            ConsoleWrite("Incorrect Value of POM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of POM_Vehicle Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }
        pom_VehicleSpawnCount = tmpValue;
    }

    if (Regex.Match(strVariable, @"POM_Player Spawn Time").Success)
    {
        int tmpValue = Convert.ToInt32(strValue);

        if (tmpValue > 100)
        {
            tmpValue = 100;
            ConsoleWrite("Incorrect Value of PM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
        }


        if (tmpValue < 5)
        {
            tmpValue = 5;
            ConsoleWrite("Incorrect Value of POM_Player Spawn Time");
            ConsoleWrite("this Setting have to be between 5 and 100");
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


    if (Regex.Match(strVariable, @"POM_Allow M9").Success)
    {
        if (strValue == "Yes") pom_allowPistol_M9 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_M9 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow QSZ-92").Success)
    {
        if (strValue == "Yes") pom_allowPistol_QSZ92 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_QSZ92 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow MP-443").Success)
    {
        if (strValue == "Yes") pom_allowPistol_MP443 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_MP443 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow SHORTY 12G").Success)
    {
        if (strValue == "Yes") pom_allowPistol_Shorty = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_Shorty = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow G18").Success)
    {
        if (strValue == "Yes") pom_allowPistol_Glock18 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_Glock18 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow FN57").Success)
    {
        if (strValue == "Yes") pom_allowPistol_FN57 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_FN57 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow M1911").Success)
    {
        if (strValue == "Yes") pom_allowPistol_M1911 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_M1911 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow 93R").Success)
    {
        if (strValue == "Yes") pom_allowPistol_93R = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_93R = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow CZ-75").Success)
    {
        if (strValue == "Yes") pom_allowPistol_CZ75 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_CZ75 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow .44 MAGNUM").Success)
    {
        if (strValue == "Yes") pom_allowPistol_Taurus44 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_Taurus44 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow COMPACT 45").Success)
    {
        if (strValue == "Yes") pom_allowPistol_HK45C = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_HK45C = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow P226").Success)
    {
        if (strValue == "Yes") pom_allowPistol_P226 = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_P226 = enumBoolYesNo.No;
    }

    if (Regex.Match(strVariable, @"POM_Allow M412 REX").Success)
    {
        if (strValue == "Yes") pom_allowPistol_MP412Rex = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_MP412Rex = enumBoolYesNo.No;
    }


    if (Regex.Match(strVariable, @"POM_Allow KNIFE").Success)
    {
        if (strValue == "Yes") pom_allowPistol_Meele = enumBoolYesNo.Yes;
        if (strValue == "No") pom_allowPistol_Meele = enumBoolYesNo.No;
    }







} // Speichern der von User eingegebenen Variablen



private void UpdateStartupMode() // Modes shown at Startup Mode in Pugin settings
{
    startup_mode_def = "enum.startup_mode(none|normal";
    if (pm_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|private";
    if (fm_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|flagrun";
    if (kom_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|knife";
    if (pom_isEnabled == enumBoolYesNo.Yes) startup_mode_def = startup_mode_def + "|pistol";
    startup_mode_def = startup_mode_def + ")";
}

public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion) {
    
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
                                             "OnPlayerAuthenticated",
                                             "OnServerMessage",
                                             "OnVehicleSpawnAllowed",
                                             "OnVehicleSpawnDelay",
                                             "OnPlayerRespawnTime"
                                             );
}


public void OnCommandTest(string strSpeaker, string strText, MatchCommand mtcCommand, CapturedCommand capCommand, CPlayerSubset subMatchedScope) // Funktion zum Testcommand
{
    WritePluginConsole("^1^b[OnCommandTest]^0^n Speaker was: "+strSpeaker+" text was: "+strText , "Info", 0);
}

public void OnTaskTriggered() // Example
{
    WritePluginConsole("^1^b[OnTaskTriggered]^0^n was Called", "Info", 0);

}


public void OnPluginEnable() {

    Thread thread_PluginEnable = new Thread(new ThreadStart(delegate()
    {
        WritePluginConsole("Init Plugin...", "Info", 0);
        Thread.Sleep(1000);
        WritePluginConsole("Set Startup Vars...", "Info", 2);
        serverMode = "plugin_init";
        next_serverMode = startup_mode;
        if (startup_mode == "none")
        {
            serverMode = "normal";
            next_serverMode = "normal";
        }
        
        /// TRY COMMAND
        this.RegisterCommand(
                    new MatchCommand(
                        "ExtraServerFuncs",
                        "OnCommandTest",
                        this.Listify<string>("@", "!", "#"),
                        "testcommand",
                        this.Listify<MatchArgumentFormat>(),
                        new ExecutionRequirements(
                            ExecutionScope.All),
                        "ONLY A TEST"
                    ));
        
        /*this.RegisterCommand(
                        new MatchCommand(
                        "ExtraServerFuncs",
                        "OnTaskTriggered",
                        new List<string>(),
                        "Triggertest",
                        //new List<MatchArgumentFormat>(),
                        new ExecutionRequirements(ExecutionScope.None),
                        "ONLY A TEST TRIGGERED BY A TASK"
                        ));
        */
        



        /// TRY COMMAND

        plugin_enabled = true;
        fIsEnabled = true;
        players = new PlayerDB();
        files = new TextDatei();

        WritePluginConsole("ENABLED - Thanks for using :)", "Info", 0);
        Thread.Sleep(1000);
        if (startup_mode != "none")SwitchServerMode(next_serverMode);
        WritePluginConsole("LOADED Startup Server Mode", "Info", 0);
        return;
    }));

    thread_PluginEnable.Start();
    
    
    
    
} 

public void OnPluginDisable() {
	plugin_enabled = false;
	fIsEnabled = false;
	ConsoleWrite("Disabled :(");

    /// TRY COMMAND
    this.UnregisterCommand(
                new MatchCommand(
                    "Extra Server Funcs",
                    "OnCommandTest",
                    this.Listify<string>("@", "!", "#"),
                    "testcommand",
                    this.Listify<MatchArgumentFormat>(),
                    new ExecutionRequirements(
                        ExecutionScope.All),
                    "ONLY A TEST"
                ));
    /// TRY COMMAND




}

public override void OnVersion(string serverType, string version) { }

public override void OnServerInfo(CServerInfo serverInfo) {
    if (plugin_enabled)
    {
        ConsoleWrite("Debug level = " + fDebugLevel);
        ConsoleWrite("Current Server Mode: "+ serverMode);
        ConsoleWrite("Next Server Mode "+ next_serverMode);
    }
}

public void OnMaplistMapInserted(int mapIndex, string mapFileName)
{
    wait = 0; // Bremse für Maplist.add aufheben
}

public void OnAnyChat(string speaker, string message)
{
    if (plugin_enabled)
    {
        if (IsAdmin(speaker) && IsCommand(message)) PluginCommand(speaker, ExtractCommand(message));
    }
    
}

public void OnServerName(string serverName)  // Server Name was changed
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginVariable("NM_Server Name", serverName); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Server Name", serverName); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
            if (serverMode == "flagrun") SetPluginVariable("FM_Server Name", serverName); // SAVE SERVERNAME TO FLAGRUN MODE CONFIG
            if (serverMode == "knife") SetPluginVariable("KOM_Server Name", serverName); // SAVE SERVERNAME TO KNIFE ONLY MODE CONFIG
            if (serverMode == "pistol") SetPluginVariable("POM_Server Name", serverName); // SAVE SERVERNAME TO PRIVATE MODE CONFIG
        }
    }
}

public void OnServerMessage(string Message)  // Server Name was changed
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginVariable("NM_Server Message", Message); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Server Message", Message); // SAVE SERVERNAME TO PRIVATE MODE CONFIG

        }
    }
}

public void OnServerDescription(string serverDescription)
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginVariable("NM_Server Description", serverDescription); // SAVE SERVERNAME TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Server Description", serverDescription); // SAVE SERVERNAME TO PRIVATE MODE CONFIG

        }
    }


}

public void OnVehicleSpawnAllowed(bool isEnabled)   // vars.vehicleSpawnAllowed
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {

            if (serverMode == "normal") SetPluginVariable("NM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled)); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Vehicle Spawn Allowed", boolToStringYesNo(isEnabled)); // SAVE TO PRIVATE MODE CONFIG

        }
    }


}

public void OnVehicleSpawnDelay(int limit)   // vars.vehicleSpawnDelay
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginVariable("NM_Vehicle Spawn Time", limit.ToString()); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Vehicle Spawn Time", limit.ToString()); // SAVE TO PRIVATE MODE CONFIG

        }
    }


}

public void OnPlayerRespawnTime(int limit)   //vars.playerRespawnTime
{
    if (plugin_enabled)
    {
        if (autoconfig == enumBoolYesNo.Yes || readconfig)
        {
            if (serverMode == "normal") SetPluginVariable("NM_Player Spawn Time", limit.ToString()); // SAVE TO NORMAL MODE CONFIG
            if (serverMode == "private") SetPluginVariable("PM_Player Spawn Time", limit.ToString()); // SAVE TO PRIVATE MODE CONFIG

        }
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
            WritePluginConsole("Caught Exception in ListsEqual", "Error", 1);
            WritePluginConsole(e.Message, "Error", 1);
            throw;
        }
        this.m_listCurrMapList = new List<MaplistEntry>(lstMaplist);
        WritePluginConsole("Maplist updated. There are " + m_listCurrMapList.Count + " maps currently in the maplist", "Info", 5);

    }



}

public override void OnResponseError(List<string> requestWords, string error)
{
    WritePluginConsole("^1^b PROCON ERROR: "+ error, "error", 2);

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
        if (!tmpplayerdb.Contains(player)) players.Remove(player); // Spieler Löschen wenn noch in der DB aber nicht mehr auf dem Server 
    
    
    
    }
}

public override void OnPlayerJoin(string soldierName) 
{

    if (plugin_enabled)
    {
        if (serverMode == "private") // Check if server is in PRIVATE MODE
        {
            if (!isInWhitelist(soldierName)) kickPlayer(soldierName, msg_pmKick, 40);  // Kick player if not in Whitelist

        }

    }
}

public override void OnPlayerAuthenticated(string soldierName, string guid)
{
    DebugWrite("[OnPlayerAuthenticated]", 6);
    players.Add(soldierName);
}

public override void OnPlayerLeft(CPlayerInfo playerInfo)
{
    players.Remove(playerInfo.SoldierName);

}

public override void OnPlayerKilled(Kill kKillerVictimDetails)
 {
    
    try
    {
    
    
    lastKiller = kKillerVictimDetails.Killer.SoldierName;
    lastVictim = kKillerVictimDetails.Victim.SoldierName;
    lastWeapon = kKillerVictimDetails.DamageType;
    lastUsedWeapon = FWeaponName(lastWeapon);

    if (showweaponcode == enumBoolYesNo.Yes) WritePluginConsole("^2^n" + lastKiller + "^1^b  [ " + lastUsedWeapon + " ]^7^n " + lastVictim, "KILL", 0); // Zeige Die Waffen in der Konsole Farblich hervorgehoben
    
    
     DebugWrite("[OnPlayerKilled] Killer:     " + lastKiller, 6);
     DebugWrite("[OnPlayerKilled] Victim:     " + lastVictim, 6);
     DebugWrite("[OnPlayerKilled] DamageType: " + lastWeapon, 6);
    
   
    if (lastKiller != "" && lastWeapon != "Suicide")
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
        WritePluginConsole("^1^bOnPlayerKilled returs an Error: ^0^n"+ ex.ToString(), "ERROR", 9);
    }

 }

private bool isprohibitedWeapon(string weapon)
{
    try
    {
        WritePluginConsole("Called isprohibitedWeapon()", "DEBUG", 10);

        if (serverMode == "flagrun")  // FLAGRUN MODE ALSO KEIN KILL ERLAUBT
        {
            WritePluginConsole("isprohibitedWeapon() is ^1^bTRUE^0^n  FLAGRUN MODE!", "DEBUG", 10);
            return true;
        }

        if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes && g_prohibitedWeapons.Contains(weapon))   // GENERELL VERBOTENE WAFFEN
        {
            WritePluginConsole("isprohibitedWeapon() is ^1^bTRUE^0^n  General prohibited Weapon match", "DEBUG", 10);
            return true;
        }

        if (serverMode == "knife" && weapon != "Melee")  // FLAGRUN MODE ALSO KEIN KILL ERLAUBT
        {
            WritePluginConsole("isprohibitedWeapon() is ^1^bTRUE^0^n  KNIFE ONLY MODE!", "DEBUG", 10);
            return true;
        }


        if (serverMode == "pistol")  // PISTOL ONLY MODE
        {
            WritePluginConsole("isprohibitedWeapon() Check Weapon: " + weapon, "DEBUG", 10);
            List<string> tmp_pistols = new List<string>();
            
            if ( pom_allowPistol_M9 == enumBoolYesNo.Yes ) tmp_pistols.Add("M9");             //M9
            if ( pom_allowPistol_QSZ92 == enumBoolYesNo.Yes ) tmp_pistols.Add("QSZ92");           //QSZ-92
            if ( pom_allowPistol_MP443 == enumBoolYesNo.Yes ) tmp_pistols.Add("MP443");            //MP-443
            if (pom_allowPistol_Shorty == enumBoolYesNo.Yes) tmp_pistols.Add("SerbuShorty");            //SHORTY 12G
            if ( pom_allowPistol_Glock18 == enumBoolYesNo.Yes ) tmp_pistols.Add("Glock18");          //G18
            if ( pom_allowPistol_FN57 == enumBoolYesNo.Yes ) tmp_pistols.Add("FN57");             //FN57
            if ( pom_allowPistol_M1911 == enumBoolYesNo.Yes ) tmp_pistols.Add("M1911");            //M1911
            if ( pom_allowPistol_93R == enumBoolYesNo.Yes ) tmp_pistols.Add("M93R");              //93R
            if ( pom_allowPistol_CZ75 == enumBoolYesNo.Yes ) tmp_pistols.Add("CZ75");             //CZ-75
            if ( pom_allowPistol_Taurus44 == enumBoolYesNo.Yes ) tmp_pistols.Add("Taurus44");         //.44 MAGNUM
            if ( pom_allowPistol_HK45C == enumBoolYesNo.Yes ) tmp_pistols.Add("HK45C");            //COMPACT 45
            if ( pom_allowPistol_P226 == enumBoolYesNo.Yes ) tmp_pistols.Add("P226");             //P226
            if ( pom_allowPistol_MP412Rex == enumBoolYesNo.Yes ) tmp_pistols.Add("MP412Rex");         //M412 REX
            if ( pom_allowPistol_Meele == enumBoolYesNo.Yes ) tmp_pistols.Add("Melee");            //Knife



            if (!tmp_pistols.Contains(weapon))
            {
                WritePluginConsole("isprohibitedWeapon() is ^1^bTRUE^0^n  PROHIBITED PISTOL! " + weapon, "DEBUG", 10);
                return true;
                
            }

        }



        
    }
    catch (Exception ex)
    {
        WritePluginConsole("^1^bisprohibitedWeapon returs an Error: ^0^n" + ex.ToString(), "ERROR", 9);
    }
    WritePluginConsole("isprohibitedWeapon() is ^1^bFALSE^0^n", "DEBUG", 10);
    return false;
}
    
private void PlayerWarn(string name,string weapon)
{
    try
    {
        int yell_Time = 10;  // In user Settings aufnehmen
        players.Warn(name);
        int warns = players.Warns(name);

        if (serverMode == "private" || serverMode == "normal")   // Muss noch angepasst werden - KNIFE braucht nen eingenen bereich
        {
            if (g_prohibitedWeapons_enable == enumBoolYesNo.Yes)  // GENERELL VERBOTENE WAFFEN
            {
                if (g_prohibitedWeapons.Contains(weapon))
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
            }



        }


        // KNIFE ONLY MODE
        if (serverMode == "knife")   // Muss noch angepasst werden - KNIFE braucht nen eingenen bereich
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

            if (warns > kom_max_Warns) // Maximale Warnungen überschritten
            {
                if (!isInWhitelist(name)) kom_Action(name);
                SendGlobalMessage(R(msg_KnifeKick));

            }

        }


        // KNIFE ONLY MODE
        if (serverMode == "pistol")   // Muss noch angepasst werden - KNIFE braucht nen eingenen bereich
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

            if (warns > pom_max_Warns) // Maximale Warnungen überschritten
            {
                if (!isInWhitelist(name)) pom_Action(name);
                SendGlobalMessage(R(msg_PistolKick));

            }

        }               


        









        if (serverMode == "flagrun")// && !isInWhitelist(name))
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
DebugWrite("Player spawn detected. Playername = " + soldierName, 6);




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
DebugWrite("[OnLoadingLevel] " + mapFileName, 5);
}

public override void OnLevelStarted()
 {
DebugWrite("[OnLevelStarted]", 5);
 }

public override void OnLevelLoaded(string mapFileName, string Gamemode, int roundsPlayed, int roundsTotal) // BF3
{
DebugWrite("[OnLevelLoaded] " + mapFileName, 5);



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

    if (!csv_db.isInit()) csv_db.Init(@"Logs\PlayerDB.csv");
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


public class BattlelogClient
    {
      private HttpWebRequest req = null;

      WebClient client = null;

      private String fetchWebPage(ref String html_data, String url)
      {
        try
        {
          if (client == null)
            client = new WebClient();

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
          fetchWebPage(ref result, "http://battlelog.battlefield.com/bf4/user/" + player);

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
    /// Liefert den Inhalt der Datei zurück.
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
    /// Schreibt den übergebenen Inhalt in eine Textdatei.
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
    /// Fügt den übergebenen Text an das Ende einer Textdatei an.
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="sLines">anzufügender Text</param>
    public void Append(string sFilename, string sLines)
    {
        StreamWriter myFile = new StreamWriter(sFilename, true);
        myFile.Write(sLines);
        myFile.Close();
    }

    ///<summary>
    /// Liefert den Inhalt der übergebenen Zeilennummer zurück.
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
    /// Schreibt den übergebenen Text in eine definierte Zeile.
    ///</summary>
    ///<param name="sFilename">Pfad zur Datei</param>
    ///<param name="iLine">Zeilennummer</param>
    ///<param name="sLines">Text für die übergebene Zeile</param>
    ///<param name="bReplace">Text in dieser Zeile überschreiben (t) oder einfügen (f)</param>
    
    
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
                if (lineCount > 1) // Erste Zeile überspringen, da es sich um den Header handelt
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
    string Savestring;
    if (!db_fileinit) return; // Breche Schreibvorgng ab wenn die Datenbank nicht Initialisiert wurde
    foreach (CSV_PlayerInfo player in csv_Players)
    {
        Savestring = player.PlayerName + ";" + player.ClanTag + ";" + Convert.ToString(player.Visits) + ";" + Convert.ToString(player.Score) + ";" + Convert.ToString(player.Kills) + ";" + Convert.ToString(player.Death) + ";" + Convert.ToString(player.Suicides) + ";" + Convert.ToString(player.Warns) + ";" + Convert.ToString(player.Kicks) + ";" + Convert.ToString(player.Endrounds) + ";" + Convert.ToString(player.LastSeen) + ";" + Convert.ToString(player.PlayedTime);

        if (player.fileline == 0) files.WriteLine(CSV_Filename, Savestring); // Spieler ist neu aus dem Server und in der Datei nicht vorhanden. Schreibe neuen Datensatz
        if (player.fileline > 1) files.WriteLine(CSV_Filename, player.fileline, Savestring, true); // Datensatz des Spieler in der CSV Datei überschreien
    }

    csv_Players.Clear();
    db_fileinit = false;
    return;
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

public class MapListProhibitedWeapons
{





}






} // end namespace PRoConEvents




