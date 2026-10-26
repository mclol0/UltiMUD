using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Ultimud
{
    [Obfuscation(Exclude = false)]
    internal class MudStarterKitForm : Form
    {
        ComboBox presetBox_;
        TextBox mudNameBox_;
        CheckedListBox packList_;
        RichTextBox logBox_;
        Button generateBtn_;

        public MudStarterKitForm()
        {
            this.Text = "UltiMUD Creator Studio — Starter Kit & World Generator";
            this.Size = new Size(760, 580);
            this.MinimumSize = new Size(680, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 68;
            headerPanel.BackColor = Color.FromArgb(20, 36, 64);
            headerPanel.Padding = new Padding(14, 10, 14, 10);

            Label titleLbl = new Label();
            titleLbl.Text = "MUD Starter Kit & World Content Generator";
            titleLbl.ForeColor = Color.White;
            titleLbl.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            titleLbl.AutoSize = true;
            titleLbl.Location = new Point(14, 10);

            Label subLbl = new Label();
            subLbl.Text = "Instantly scaffold balanced RPG stats, races, classes, abilities, weapons, potions, NPCs, shops, and a playable starter town.";
            subLbl.ForeColor = Color.FromArgb(186, 210, 245);
            subLbl.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            subLbl.AutoSize = true;
            subLbl.Location = new Point(14, 36);

            headerPanel.Controls.Add(titleLbl);
            headerPanel.Controls.Add(subLbl);

            Panel bodyPanel = new Panel();
            bodyPanel.Dock = DockStyle.Fill;
            bodyPanel.Padding = new Padding(14);

            // Top row: MUD Name + Theme Preset
            Panel configRow = new Panel();
            configRow.Dock = DockStyle.Top;
            configRow.Height = 40;

            Label nameLbl = new Label();
            nameLbl.Text = "MUD World Name:";
            nameLbl.AutoSize = true;
            nameLbl.Location = new Point(0, 8);
            nameLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.mudNameBox_ = new TextBox();
            this.mudNameBox_.Location = new Point(118, 5);
            this.mudNameBox_.Width = 220;
            this.mudNameBox_.Font = new Font("Segoe UI", 9.5F);
            if (Server.Instance != null && Server.Instance.Settings != null && !string.IsNullOrEmpty(Server.Instance.Settings.Name))
                this.mudNameBox_.Text = Server.Instance.Settings.Name;
            else
                this.mudNameBox_.Text = "Realms of Aethelgard";

            Label presetLbl = new Label();
            presetLbl.Text = "Theme Preset:";
            presetLbl.AutoSize = true;
            presetLbl.Location = new Point(360, 8);
            presetLbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.presetBox_ = new ComboBox();
            this.presetBox_.DropDownStyle = ComboBoxStyle.DropDownList;
            this.presetBox_.Location = new Point(455, 5);
            this.presetBox_.Width = 240;
            this.presetBox_.Font = new Font("Segoe UI", 9.5F);
            this.presetBox_.Items.AddRange(new object[]
            {
                "Complete RPG Starter Kit (All 8 Packs)",
                "Core Mechanics Only (Stats, Formulas & Commands)",
                "Races, Classes & Abilities Only",
                "Items, NPCs & Starter Town Map Only"
            });
            this.presetBox_.SelectedIndex = 0;
            this.presetBox_.SelectedIndexChanged += new EventHandler(this.presetBox__SelectedIndexChanged);

            configRow.Controls.Add(nameLbl);
            configRow.Controls.Add(this.mudNameBox_);
            configRow.Controls.Add(presetLbl);
            configRow.Controls.Add(this.presetBox_);

            // Split: CheckedListBox of Packs + Output Summary Log
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 235;

            GroupBox packGroup = new GroupBox();
            packGroup.Text = "Select Content Packs to Install (Existing objects with the same name are preserved)";
            packGroup.Dock = DockStyle.Fill;
            packGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.packList_ = new CheckedListBox();
            this.packList_.Dock = DockStyle.Fill;
            this.packList_.CheckOnClick = true;
            this.packList_.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.packList_.Items.Add("1. Core RPG Stat Blueprints & Lua Formula Scripts (hp, mp, mv, str, dex, int, con, armor, level)", true);
            this.packList_.Items.Add("2. Playable Races Pack (Human, Elf, Dwarf, Half-Orc, Halfling, Mystic + Starting Modifiers)", true);
            this.packList_.Items.Add("3. Playable Classes / Jobs Pack (Warrior, Mage, Rogue, Cleric, Ranger, Paladin + Gear/Skills)", true);
            this.packList_.Items.Add("4. Combat Abilities, Spells & Status Effects Pack (Slash, Fireball, Heal, Backstab, Second Wind, etc.)", true);
            this.packList_.Items.Add("5. Starter Weapons, Armor, Containers & Potions Pack (Longsword, Staff, Armor, Healing Potion, etc.)", true);
            this.packList_.Items.Add("6. Town NPCs, Skill Trainers & Dungeon Monsters Pack (Guards, Healer, Guildmaster, Goblin, Dragon, etc.)", true);
            this.packList_.Items.Add("7. Starter Town Map Layout (Cobblestone Roads, Safe Town Square, Blacksmith Shop & Monster Spawns)", true);
            this.packList_.Items.Add("8. Ensure All 100 Default Lua .cmd Scripts, Help Files, Channels & Socials", true);
            packGroup.Controls.Add(this.packList_);
            split.Panel1.Controls.Add(packGroup);

            GroupBox logGroup = new GroupBox();
            logGroup.Text = "Generator Log & Installed Blueprints";
            logGroup.Dock = DockStyle.Fill;
            logGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            this.logBox_ = new RichTextBox();
            this.logBox_.Dock = DockStyle.Fill;
            this.logBox_.ReadOnly = true;
            this.logBox_.BackColor = Color.FromArgb(15, 23, 42);
            this.logBox_.ForeColor = Color.FromArgb(226, 232, 240);
            this.logBox_.Font = new Font("Consolas", 9.5F, FontStyle.Regular);
            this.logBox_.Text = "Ready. Select the starter packs above and click 'Generate Selected Packs into MUD'.\r\n";
            logGroup.Controls.Add(this.logBox_);
            split.Panel2.Controls.Add(logGroup);

            // Bottom Action Bar
            Panel bottomBar = new Panel();
            bottomBar.Dock = DockStyle.Bottom;
            bottomBar.Height = 46;
            bottomBar.Padding = new Padding(0, 8, 0, 0);

            this.generateBtn_ = new Button();
            this.generateBtn_.Text = "✦ Generate Selected Packs into MUD";
            this.generateBtn_.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.generateBtn_.BackColor = Color.FromArgb(22, 163, 74);
            this.generateBtn_.ForeColor = Color.White;
            this.generateBtn_.FlatStyle = FlatStyle.Flat;
            this.generateBtn_.FlatAppearance.BorderSize = 0;
            this.generateBtn_.Width = 265;
            this.generateBtn_.Height = 34;
            this.generateBtn_.Dock = DockStyle.Right;
            this.generateBtn_.Cursor = Cursors.Hand;
            this.generateBtn_.Click += new EventHandler(this.generateBtn__Click);

            Button closeBtn = new Button();
            closeBtn.Text = "Close";
            closeBtn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            closeBtn.Width = 90;
            closeBtn.Height = 34;
            closeBtn.Dock = DockStyle.Left;
            closeBtn.Click += delegate { this.Close(); };

            bottomBar.Controls.Add(this.generateBtn_);
            bottomBar.Controls.Add(closeBtn);

            bodyPanel.Controls.Add(split);
            bodyPanel.Controls.Add(configRow);
            bodyPanel.Controls.Add(bottomBar);

            this.Controls.Add(bodyPanel);
            this.Controls.Add(headerPanel);
        }

        void presetBox__SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = this.presetBox_.SelectedIndex;
            for (int i = 0; i < this.packList_.Items.Count; i++)
            {
                bool check = true;
                if (idx == 1)
                    check = (i == 0 || i == 7);
                else if (idx == 2)
                    check = (i == 0 || i == 1 || i == 2 || i == 3);
                else if (idx == 3)
                    check = (i == 0 || i == 4 || i == 5 || i == 6);
                this.packList_.SetItemChecked(i, check);
            }
        }

        void Log(string msg)
        {
            this.logBox_.AppendText(msg + "\r\n");
            this.logBox_.SelectionStart = this.logBox_.TextLength;
            this.logBox_.ScrollToCaret();
        }

        void generateBtn__Click(object sender, EventArgs e)
        {
            Server srv = Server.Instance;
            if (srv == null)
            {
                MessageBox.Show(this, "No MUD server is currently loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.logBox_.Clear();
            string newName = this.mudNameBox_.Text.Trim();
            if (!string.IsNullOrEmpty(newName) && (string.IsNullOrEmpty(srv.Settings.Name) || srv.Settings.Name == "Unnamed"))
            {
                srv.Settings.Name = newName;
                this.Log("[World] Set MUD World Name to '" + newName + "'.");
            }

            if (srv.FightSystem == null)
            {
                srv.FightSystem = new AutomatedFightSystem();
                this.Log("[Combat] Configured default AutomatedFightSystem.");
            }

            if (srv.MainArea != null)
            {
                srv.MainArea.AreaFlags.Set((int)Area.AreaFlag.StartingArea, true);
                if (srv.MainArea.Name == "Default Area")
                    srv.MainArea.Name = "haven";
            }

            int createdCount = 0;

            // Pack 1: Stats & Formula Scripts
            if (this.packList_.GetItemChecked(0))
                createdCount += InstallStatsAndFormulas(srv);

            // Pack 4 before 2 & 3 so Races/Jobs can reference Abilities & Items!
            if (this.packList_.GetItemChecked(3))
                createdCount += InstallAbilitiesAndStatusEffects(srv);

            // Pack 5: Starter Items
            if (this.packList_.GetItemChecked(4))
                createdCount += InstallStarterItems(srv);

            // Pack 2: Playable Races
            if (this.packList_.GetItemChecked(1))
                createdCount += InstallPlayableRaces(srv);

            // Pack 3: Playable Jobs / Classes
            if (this.packList_.GetItemChecked(2))
                createdCount += InstallPlayableJobs(srv);

            // Pack 6: Starter NPCs & Monsters
            if (this.packList_.GetItemChecked(5))
                createdCount += InstallStarterNpcs(srv);

            // Pack 7: Starter Town Map Layout
            if (this.packList_.GetItemChecked(6))
                createdCount += InstallStarterTownMap(srv);

            // Pack 8: Commands, Help, Channels & Socials
            if (this.packList_.GetItemChecked(7))
                createdCount += InstallCommandsHelpChannelsSocials(srv);

            srv.Save();
            if (MainForm.Instance != null)
                MainForm.Instance.RefreshAllViews();

            this.Log("------------------------------------------------------------");
            this.Log("COMPLETE! Created/updated " + createdCount + " blueprints and saved MUD.");
            MessageBox.Show(this,
                "Starter Kit generation complete!\r\nCreated/configured " + createdCount + " MUD blueprints and saved your world.",
                "MUD Starter Kit Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        static T FindOrCreate<T>(Server srv, string name, out bool isNew) where T : class, IIdentifiable, new()
        {
            isNew = false;
            T existing = srv.Get<T>(name);
            if (existing != null)
                return existing;

            T obj = new T();
            obj.Id = srv.NewGuid();
            obj.Name = name;
            srv.Add<T>(obj);
            isNew = true;
            return obj;
        }

        static Script EnsureFormulaScript(Server srv, string name, string luaCode, Script.Types type)
        {
            Script existing = srv.Get<Script>(name);
            if (existing != null)
                return existing;

            Script s = new Script(type, name, luaCode);
            s.Id = srv.NewGuid();
            srv.Add<Script>(s);
            return s;
        }

        static StatModifier CreateStatMod(StatBlueprint stat, Script formula, StatModifier.ApplyTarget applyTo)
        {
            StatModifier sm = new StatModifier();
            if (stat != null)
                sm.StatId = stat.Id;
            if (formula != null)
                sm.Formula = formula.Id;
            sm.ApplyTo = applyTo;
            sm.Chance = 100;
            return sm;
        }

        int InstallStatsAndFormulas(Server srv)
        {
            int count = 0;
            bool isNew;

            StatBlueprint hp = FindOrCreate<StatBlueprint>(srv, "hp", out isNew);
            if (isNew)
            {
                hp.Min = 0;
                hp.Max = 99999;
                hp.StartCurrent = 100;
                hp.StartMax = 100;
                hp.RegenFrequency = 10;
                hp.RegenAmount = 5;
                hp.OnMin = new Object.TriggerAction(Object.Die);
                hp.CanRestore = true;
                hp.Visible = true;
                count++;
            }

            StatBlueprint mp = FindOrCreate<StatBlueprint>(srv, "mp", out isNew);
            if (isNew)
            {
                mp.Min = 0;
                mp.Max = 99999;
                mp.StartCurrent = 100;
                mp.StartMax = 100;
                mp.RegenFrequency = 10;
                mp.RegenAmount = 8;
                mp.CanRestore = true;
                mp.Visible = true;
                count++;
            }

            StatBlueprint mv = FindOrCreate<StatBlueprint>(srv, "mv", out isNew);
            if (isNew)
            {
                mv.Min = 0;
                mv.Max = 99999;
                mv.StartCurrent = 100;
                mv.StartMax = 100;
                mv.RegenFrequency = 8;
                mv.RegenAmount = 15;
                mv.CanRestore = true;
                mv.Visible = true;
                count++;
            }

            string[] attrStats = new string[] { "str", "dex", "int", "wis", "con", "armor", "level", "xp" };
            foreach (string stName in attrStats)
            {
                StatBlueprint sb = FindOrCreate<StatBlueprint>(srv, stName, out isNew);
                if (isNew)
                {
                    sb.Min = 0;
                    sb.Max = (stName == "xp" ? 10000000 : 1000);
                    sb.StartCurrent = (stName == "level" ? 1 : (stName == "xp" || stName == "armor" ? 0 : 10));
                    sb.StartMax = (stName == "xp" ? 1000 : (stName == "level" ? 100 : 10));
                    sb.CanRestore = false;
                    sb.Visible = true;
                    count++;
                }
            }

            // Reusable Lua StatModifier Formula Scripts
            EnsureFormulaScript(srv, "Formula: +2", "return 2", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: +5", "return 5", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: +10", "return 10", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: +25", "return 25", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: +50", "return 50", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: -10 Cost", "return -10", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: -15 Cost", "return -15", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: -20 Cost", "return -20", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: Melee Strike Damage", "return -math.random(12, 22)", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: Heavy Skill Damage", "return -math.random(24, 38)", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: Fireball Spell Damage", "return -math.random(28, 45)", Script.Types.StatModifier);
            EnsureFormulaScript(srv, "Formula: Heal Restore (+40)", "return math.random(35, 50)", Script.Types.StatModifier);

            this.Log("[Pack 1] Configured Core RPG Stat Blueprints (hp, mp, mv, str, dex, int, wis, con, armor, level, xp) + 12 Lua Formula Scripts.");
            return count;
        }

        int InstallAbilitiesAndStatusEffects(Server srv)
        {
            int count = 0;
            bool isNew;

            StatBlueprint hp = srv.Get<StatBlueprint>("hp");
            StatBlueprint mp = srv.Get<StatBlueprint>("mp");
            StatBlueprint mv = srv.Get<StatBlueprint>("mv");

            Script cost10 = EnsureFormulaScript(srv, "Formula: -10 Cost", "return -10", Script.Types.StatModifier);
            Script cost15 = EnsureFormulaScript(srv, "Formula: -15 Cost", "return -15", Script.Types.StatModifier);
            Script cost20 = EnsureFormulaScript(srv, "Formula: -20 Cost", "return -20", Script.Types.StatModifier);
            Script dmgMelee = EnsureFormulaScript(srv, "Formula: Melee Strike Damage", "return -math.random(12, 22)", Script.Types.StatModifier);
            Script dmgHeavy = EnsureFormulaScript(srv, "Formula: Heavy Skill Damage", "return -math.random(24, 38)", Script.Types.StatModifier);
            Script dmgFire = EnsureFormulaScript(srv, "Formula: Fireball Spell Damage", "return -math.random(28, 45)", Script.Types.StatModifier);
            Script heal40 = EnsureFormulaScript(srv, "Formula: Heal Restore (+40)", "return math.random(35, 50)", Script.Types.StatModifier);

            // Status Effects
            StatusEffectBlueprint poisoned = FindOrCreate<StatusEffectBlueprint>(srv, "Poisoned", out isNew);
            if (isNew)
            {
                poisoned.StartMessage = new String("Venom courses through your veins!", "Venom courses through your veins!", null, null, "{1} shudders as poison takes hold!", null);
                poisoned.UpdateMessage = new String("You suffer from poison!", "You suffer from poison!", null, null, "{1} winces from poison.", null);
                poisoned.EndMessage = new String("The poison wears off.", "The poison wears off.", null, null, "{1} looks relieved as the poison fades.", null);
                count++;
            }

            StatusEffectBlueprint blessed = FindOrCreate<StatusEffectBlueprint>(srv, "Blessed", out isNew);
            if (isNew)
            {
                blessed.StartMessage = new String("A golden aura of blessing surrounds you.", "A golden aura of blessing surrounds you.", null, null, "{1} is surrounded by a golden aura.", null);
                blessed.EndMessage = new String("Your blessing fades.", "Your blessing fades.", null, null, "The golden aura around {1} fades.", null);
                count++;
            }

            // Abilities
            Ability slash = FindOrCreate<Ability>(srv, "slash", out isNew);
            if (isNew)
            {
                slash.Target = Ability.Targets.Others;
                slash.Style = Ability.Styles.Aggressive;
                slash.RangeMax = 0;
                slash.RecoverTime = 2;
                slash.TargetTypes.Set((int)Ability.TargetType.Npc, true);
                slash.TargetTypes.Set((int)Ability.TargetType.Player, true);
                slash.PerformMessage = new String("You swing a swift slash at {2}!", null, "{1} swings a swift slash at you!", null, "{1} swings a swift slash at {2}!", null);
                slash.SuccessMessage = new String("{color:brightred}Your slash cuts deep into {2}!{color:reset}", null, "{color:brightred}{1}'s slash cuts deep into you!{color:reset}", null, "{1}'s slash cuts into {2}!", null);
                slash.MissMessage = new String("Your slash misses {2}.", null, "{1}'s slash misses you.", null, "{1}'s slash misses {2}.", null);
                if (mv != null) slash.StatCost.Add(CreateStatMod(mv, cost10, StatModifier.ApplyTarget.Current));
                if (hp != null) slash.TargetStatMods.Add(CreateStatMod(hp, dmgMelee, StatModifier.ApplyTarget.Current));
                count++;
            }

            Ability bash = FindOrCreate<Ability>(srv, "bash", out isNew);
            if (isNew)
            {
                bash.Target = Ability.Targets.Others;
                bash.Style = Ability.Styles.Aggressive;
                bash.RangeMax = 0;
                bash.RecoverTime = 3;
                bash.TargetTypes.Set((int)Ability.TargetType.Npc, true);
                bash.TargetTypes.Set((int)Ability.TargetType.Player, true);
                bash.PerformMessage = new String("You charge and slam into {2}!", null, "{1} charges and slams into you!", null, "{1} slams into {2}!", null);
                bash.SuccessMessage = new String("{color:brightyellow}Your powerful bash staggers {2}!{color:reset}", null, "{color:brightred}{1}'s bash staggers you!{color:reset}", null, "{1}'s bash staggers {2}!", null);
                if (mv != null) bash.StatCost.Add(CreateStatMod(mv, cost15, StatModifier.ApplyTarget.Current));
                if (hp != null) bash.TargetStatMods.Add(CreateStatMod(hp, dmgHeavy, StatModifier.ApplyTarget.Current));
                count++;
            }

            Ability fireball = FindOrCreate<Ability>(srv, "fireball", out isNew);
            if (isNew)
            {
                fireball.Shortcut = "fb";
                fireball.Target = Ability.Targets.Others;
                fireball.Style = Ability.Styles.Aggressive;
                fireball.RangeMax = 2;
                fireball.PrepareTime = 1;
                fireball.RecoverTime = 2;
                fireball.TargetTypes.Set((int)Ability.TargetType.Npc, true);
                fireball.TargetTypes.Set((int)Ability.TargetType.Player, true);
                fireball.PrepareMessage = new String("{color:brightyellow}You weave spheres of roaring flame...{color:reset}", null, null, null, "{1} weaves spheres of roaring flame...", null);
                fireball.PerformMessage = new String("You hurl a blazing fireball at {2}!", null, "{1} hurls a blazing fireball at you!", null, "{1} hurls a blazing fireball at {2}!", null);
                fireball.SuccessMessage = new String("{color:brightred}Your fireball engulfs {2} in searing flames!{color:reset}", null, "{color:brightred}{1}'s fireball engulfs you in searing flames!{color:reset}", null, "{1}'s fireball engulfs {2}!", null);
                if (mp != null) fireball.StatCost.Add(CreateStatMod(mp, cost20, StatModifier.ApplyTarget.Current));
                if (hp != null) fireball.TargetStatMods.Add(CreateStatMod(hp, dmgFire, StatModifier.ApplyTarget.Current));
                count++;
            }

            Ability heal = FindOrCreate<Ability>(srv, "heal", out isNew);
            if (isNew)
            {
                heal.Target = Ability.Targets.All;
                heal.Style = Ability.Styles.Defensive;
                heal.RangeMax = 1;
                heal.RecoverTime = 2;
                heal.TargetTypes.Set((int)Ability.TargetType.Player, true);
                heal.TargetTypes.Set((int)Ability.TargetType.Npc, true);
                heal.PerformMessage = new String("You invoke a radiant prayer of healing...", "You invoke a radiant prayer of healing upon {2}...", "{1} invokes a radiant prayer of healing upon you...", null, "{1} invokes a prayer of healing.", null);
                heal.SuccessMessage = new String("{color:brightgreen}Warm golden light mends your wounds!{color:reset}", "{color:brightgreen}Warm golden light mends {2}'s wounds!{color:reset}", "{color:brightgreen}{1}'s golden light mends your wounds!{color:reset}", null, "Golden light surrounds {2}.", null);
                if (mp != null) heal.StatCost.Add(CreateStatMod(mp, cost15, StatModifier.ApplyTarget.Current));
                if (hp != null) heal.TargetStatMods.Add(CreateStatMod(hp, heal40, StatModifier.ApplyTarget.Current));
                count++;
            }

            Ability backstab = FindOrCreate<Ability>(srv, "backstab", out isNew);
            if (isNew)
            {
                backstab.Shortcut = "bs";
                backstab.Target = Ability.Targets.Others;
                backstab.Style = Ability.Styles.Aggressive;
                backstab.RangeMax = 0;
                backstab.RecoverTime = 3;
                backstab.TargetTypes.Set((int)Ability.TargetType.Npc, true);
                backstab.TargetTypes.Set((int)Ability.TargetType.Player, true);
                backstab.PerformMessage = new String("You step into the shadows behind {2}...", null, "{1} lunges from the shadows behind you!", null, "{1} strikes {2} from the shadows!", null);
                backstab.SuccessMessage = new String("{color:brightmagenta}Your blade strikes a vital point on {2}!{color:reset}", null, "{color:brightred}{1}'s blade strikes a vital point!{color:reset}", null, "{1} lands a critical backstab on {2}!", null);
                if (mv != null) backstab.StatCost.Add(CreateStatMod(mv, cost15, StatModifier.ApplyTarget.Current));
                if (hp != null) backstab.TargetStatMods.Add(CreateStatMod(hp, dmgHeavy, StatModifier.ApplyTarget.Current));
                count++;
            }

            this.Log("[Pack 4] Installed Status Effects (Poisoned, Blessed) & Combat Abilities (slash, bash, fireball, heal, backstab).");
            return count;
        }

        int InstallStarterItems(Server srv)
        {
            int count = 0;
            bool isNew;

            StatBlueprint hp = srv.Get<StatBlueprint>("hp");
            StatBlueprint str = srv.Get<StatBlueprint>("str");
            StatBlueprint intel = srv.Get<StatBlueprint>("int");
            StatBlueprint dex = srv.Get<StatBlueprint>("dex");
            StatBlueprint armor = srv.Get<StatBlueprint>("armor");

            Script plus2 = EnsureFormulaScript(srv, "Formula: +2", "return 2", Script.Types.StatModifier);
            Script plus5 = EnsureFormulaScript(srv, "Formula: +5", "return 5", Script.Types.StatModifier);
            Script heal40 = EnsureFormulaScript(srv, "Formula: Heal Restore (+40)", "return math.random(35, 50)", Script.Types.StatModifier);

            Item sword = FindOrCreate<Item>(srv, "iron longsword", out isNew);
            if (isNew)
            {
                sword.EquipLocation = Character.BodyParts.Held;
                sword.Value = 80;
                sword.Description = "A balanced iron longsword forged at the Haven smithy.";
                if (str != null) sword.UserStatMods.Add(CreateStatMod(str, plus2, StatModifier.ApplyTarget.CurrentAndMax));
                count++;
            }

            Item staff = FindOrCreate<Item>(srv, "apprentice oak staff", out isNew);
            if (isNew)
            {
                staff.EquipLocation = Character.BodyParts.Held;
                staff.Value = 75;
                staff.Description = "A carved oak staff topped with a faintly glowing mana crystal.";
                if (intel != null) staff.UserStatMods.Add(CreateStatMod(intel, plus2, StatModifier.ApplyTarget.CurrentAndMax));
                count++;
            }

            Item dagger = FindOrCreate<Item>(srv, "steel dirk", out isNew);
            if (isNew)
            {
                dagger.EquipLocation = Character.BodyParts.Held;
                dagger.Value = 65;
                dagger.Description = "A razor-sharp steel dirk favored by scouts and rogues.";
                if (dex != null) dagger.UserStatMods.Add(CreateStatMod(dex, plus2, StatModifier.ApplyTarget.CurrentAndMax));
                count++;
            }

            Item tunic = FindOrCreate<Item>(srv, "adventurer leather tunic", out isNew);
            if (isNew)
            {
                tunic.EquipLocation = Character.BodyParts.Body;
                tunic.Value = 60;
                tunic.Description = "Sturdy boiled leather armor offering reliable protection.";
                if (armor != null) tunic.UserStatMods.Add(CreateStatMod(armor, plus5, StatModifier.ApplyTarget.CurrentAndMax));
                count++;
            }

            Item shield = FindOrCreate<Item>(srv, "wooden buckler", out isNew);
            if (isNew)
            {
                shield.EquipLocation = Character.BodyParts.Hands;
                shield.Value = 45;
                shield.Description = "A round oak buckler reinforced with an iron rim.";
                if (armor != null) shield.UserStatMods.Add(CreateStatMod(armor, plus2, StatModifier.ApplyTarget.CurrentAndMax));
                count++;
            }

            Item potion = FindOrCreate<Item>(srv, "minor healing potion", out isNew);
            if (isNew)
            {
                potion.Value = 25;
                potion.ObjectFlags.Set((int)Object.ObjFlags.Edible, true);
                potion.Description = "A ruby-red alchemical draught that restores lost health when used or drunk.";
                if (hp != null) potion.UserStatMods.Add(CreateStatMod(hp, heal40, StatModifier.ApplyTarget.Current));
                potion.OnUse = "actor:Send('{color:brightgreen}You drink the minor healing potion and feel revitalized!{color:reset}')";
                count++;
            }

            Item pack = FindOrCreate<Item>(srv, "traveler backpack", out isNew);
            if (isNew)
            {
                pack.EquipLocation = Character.BodyParts.Back;
                pack.Value = 30;
                pack.ObjectFlags.Set((int)Object.ObjFlags.Container, true);
                pack.Size = 50;
                pack.Description = "A roomy canvas and leather backpack for carrying loot.";
                count++;
            }

            this.Log("[Pack 5] Installed Starter Items (iron longsword, apprentice oak staff, steel dirk, leather tunic, wooden buckler, minor healing potion, traveler backpack).");
            return count;
        }

        int InstallPlayableRaces(Server srv)
        {
            int count = 0;
            bool isNew;

            StatBlueprint hp = srv.Get<StatBlueprint>("hp");
            StatBlueprint mp = srv.Get<StatBlueprint>("mp");
            StatBlueprint mv = srv.Get<StatBlueprint>("mv");
            StatBlueprint str = srv.Get<StatBlueprint>("str");
            StatBlueprint dex = srv.Get<StatBlueprint>("dex");
            StatBlueprint intel = srv.Get<StatBlueprint>("int");
            StatBlueprint con = srv.Get<StatBlueprint>("con");

            Script plus100 = EnsureFormulaScript(srv, "Formula: +100", "return 100", Script.Types.StatModifier);
            Script plus10 = EnsureFormulaScript(srv, "Formula: +10", "return 10", Script.Types.StatModifier);
            Script plus25 = EnsureFormulaScript(srv, "Formula: +25", "return 25", Script.Types.StatModifier);

            string[][] races = new string[][]
            {
                new string[] { "Human", "str" },
                new string[] { "Elf", "int" },
                new string[] { "Dwarf", "con" },
                new string[] { "Half-Orc", "str" },
                new string[] { "Halfling", "dex" }
            };

            foreach (string[] rDef in races)
            {
                Race r = FindOrCreate<Race>(srv, rDef[0], out isNew);
                if (isNew)
                {
                    r.Selectable = true;
                    r.StartMoney = 200;
                    if (srv.MainArea != null)
                        r.StartLocation = new Location(srv.MainArea, srv.MainArea.Start);

                    if (hp != null) r.BaseStatModifiers.Add(CreateStatMod(hp, plus100, StatModifier.ApplyTarget.CurrentAndMax));
                    if (mp != null) r.BaseStatModifiers.Add(CreateStatMod(mp, plus100, StatModifier.ApplyTarget.CurrentAndMax));
                    if (mv != null) r.BaseStatModifiers.Add(CreateStatMod(mv, plus100, StatModifier.ApplyTarget.CurrentAndMax));
                    if (str != null) r.BaseStatModifiers.Add(CreateStatMod(str, plus10, StatModifier.ApplyTarget.CurrentAndMax));
                    if (dex != null) r.BaseStatModifiers.Add(CreateStatMod(dex, plus10, StatModifier.ApplyTarget.CurrentAndMax));
                    if (intel != null) r.BaseStatModifiers.Add(CreateStatMod(intel, plus10, StatModifier.ApplyTarget.CurrentAndMax));
                    if (con != null) r.BaseStatModifiers.Add(CreateStatMod(con, plus10, StatModifier.ApplyTarget.CurrentAndMax));

                    StatBlueprint bonusStat = srv.Get<StatBlueprint>(rDef[1]);
                    if (bonusStat != null)
                        r.BaseStatModifiers.Add(CreateStatMod(bonusStat, plus25, StatModifier.ApplyTarget.CurrentAndMax));

                    count++;
                }
            }

            this.Log("[Pack 2] Installed 5 Playable Races (Human, Elf, Dwarf, Half-Orc, Halfling) with starting stats and gold.");
            return count;
        }

        int InstallPlayableJobs(Server srv)
        {
            int count = 0;
            bool isNew;

            Item sword = srv.Get<Item>("iron longsword");
            Item staff = srv.Get<Item>("apprentice oak staff");
            Item dagger = srv.Get<Item>("steel dirk");
            Item tunic = srv.Get<Item>("adventurer leather tunic");
            Item potion = srv.Get<Item>("minor healing potion");

            Ability slash = srv.Get<Ability>("slash");
            Ability bash = srv.Get<Ability>("bash");
            Ability fireball = srv.Get<Ability>("fireball");
            Ability heal = srv.Get<Ability>("heal");
            Ability backstab = srv.Get<Ability>("backstab");

            object[][] jobs = new object[][]
            {
                new object[] { "Warrior", sword, new Ability[] { slash, bash } },
                new object[] { "Mage", staff, new Ability[] { fireball, heal } },
                new object[] { "Rogue", dagger, new Ability[] { backstab, slash } },
                new object[] { "Cleric", staff, new Ability[] { heal, bash } },
                new object[] { "Ranger", sword, new Ability[] { slash, heal } },
                new object[] { "Paladin", sword, new Ability[] { slash, bash, heal } }
            };

            foreach (object[] jDef in jobs)
            {
                string jName = (string)jDef[0];
                Item weapon = (Item)jDef[1];
                Ability[] abils = (Ability[])jDef[2];

                Job job = FindOrCreate<Job>(srv, jName, out isNew);
                if (isNew)
                {
                    job.Selectable = true;
                    job.StartMoney = 150;
                    if (srv.MainArea != null)
                        job.StartLocation = new Location(srv.MainArea, srv.MainArea.Start);

                    job.StartItems = new List<Guid>();
                    if (weapon != null) job.StartItems.Add(weapon.Id);
                    if (tunic != null) job.StartItems.Add(tunic.Id);
                    if (potion != null) job.StartItems.Add(potion.Id);

                    job.Abilities = new List<Guid>();
                    foreach (Ability a in abils)
                    {
                        if (a != null && !job.Abilities.Contains(a.Id))
                            job.Abilities.Add(a.Id);
                    }
                    count++;
                }
            }

            this.Log("[Pack 3] Installed 6 Playable Classes/Jobs (Warrior, Mage, Rogue, Cleric, Ranger, Paladin) with starting weapons, armor, potions & abilities.");
            return count;
        }

        int InstallStarterNpcs(Server srv)
        {
            int count = 0;
            bool isNew;

            StatBlueprint hp = srv.Get<StatBlueprint>("hp");
            StatBlueprint mp = srv.Get<StatBlueprint>("mp");
            StatBlueprint mv = srv.Get<StatBlueprint>("mv");

            Script xpReward = EnsureFormulaScript(srv, "NPC: Grant Victory Gold & XP",
                "if killer ~= nil then\r\n" +
                "  killer.Value = killer.Value + 35\r\n" +
                "  killer:Send('{color:brightyellow}You loot 35 gold coins from ' .. npc.Name .. '!{color:reset}')\r\n" +
                "end", Script.Types.Npc);

            // Friendly Town Guard
            Npc guard = FindOrCreate<Npc>(srv, "Captain Vane", out isNew);
            if (isNew)
            {
                guard.Description = "Captain Vane of the Haven Watch stands vigilant in polished plate armor.";
                guard.MapSymbol = "G ";
                guard.MapColor = Telnet.Color.BrightCyan;
                guard.Value = 250;
                SetCharStat(guard, hp, 350);
                SetCharStat(guard, mp, 100);
                SetCharStat(guard, mv, 200);
                guard.TextTriggers.Add(new TextTrigger("hello|hi|hail|greetings", "say Hail adventurer! Keep your blade sharp if you venture north outside the town walls."));
                guard.TextTriggers.Add(new TextTrigger("help|quest|where", "say Visit the Blacksmith Shop to buy gear, or speak with Guildmaster Kael to practice abilities!"));
                count++;
            }

            // Friendly Trainer NPC
            Npc trainer = FindOrCreate<Npc>(srv, "Guildmaster Kael", out isNew);
            if (isNew)
            {
                trainer.Description = "An experienced battle-mage who trains new adventurers in combat arts and spells.";
                trainer.MapSymbol = "T ";
                trainer.MapColor = Telnet.Color.BrightMagenta;
                trainer.TeachTrigger = Npc.TeachTriggers.OnRequest;
                SetCharStat(trainer, hp, 500);
                foreach (IIdentifiable idObj in srv.DataSets[typeof(Ability)].Values)
                {
                    trainer.TeachableAbilities[idObj.Id] = 50;
                }
                trainer.TextTriggers.Add(new TextTrigger("hello|hi|train|learn", "say Greetings! Type 'list' or 'train' here to learn new combat abilities and spells."));
                count++;
            }

            // Dungeon & Outskirts Monsters
            object[][] monsters = new object[][]
            {
                new object[] { "sewer rat", "r ", Telnet.Color.Yellow, 45, 15, false, "A mangy giant rat with gleaming red eyes." },
                new object[] { "goblin scout", "g ", Telnet.Color.Green, 85, 35, false, "A sneaky goblin scout clutching a notched blade." },
                new object[] { "crypt skeleton", "s ", Telnet.Color.White, 130, 60, true, "Ananimated skeleton warrior rattling with ancient malice." },
                new object[] { "crimson wyrmling", "W ", Telnet.Color.BrightRed, 320, 200, true, "A fierce young dragon whose scales radiate blistering heat." }
            };

            foreach (object[] mDef in monsters)
            {
                Npc mob = FindOrCreate<Npc>(srv, (string)mDef[0], out isNew);
                if (isNew)
                {
                    mob.MapSymbol = (string)mDef[1];
                    mob.MapColor = (Telnet.Color)mDef[2];
                    SetCharStat(mob, hp, (int)mDef[3]);
                    SetCharStat(mob, mp, 50);
                    SetCharStat(mob, mv, 100);
                    mob.Value = (int)mDef[4];
                    if ((bool)mDef[5])
                        mob.NpcFlags.Set((int)Npc.Flags.Aggressive, true);
                    mob.Description = (string)mDef[6];
                    mob.OnDefeat = xpReward.Id;
                    count++;
                }
            }

            this.Log("[Pack 6] Installed Town NPCs (Captain Vane, Guildmaster Kael) & Monsters (sewer rat, goblin scout, crypt skeleton, crimson wyrmling).");
            return count;
        }

        static void SetCharStat(Character ch, StatBlueprint bp, long val)
        {
            if (ch == null || bp == null)
                return;
            Stat s = new Stat(bp.Id);
            s.Max = val;
            s.Current = val;
            ch.Stats[bp.Id] = s;
        }

        int InstallStarterTownMap(Server srv)
        {
            Area area = srv.MainArea;
            if (area == null)
                return 0;

            int count = 0;
            area.AreaFlags.Set((int)Area.AreaFlag.StartingArea, true);
            area.FillChar = ',';
            area.FillColor = Telnet.Color.Green;
            area.Start = new Point(0, 0);

            // Check if we already placed the Haven General Store
            bool hasShop = false;
            foreach (Object child in area.Children)
            {
                if (child is Shop)
                {
                    hasShop = true;
                    break;
                }
            }

            if (!hasShop)
            {
                // 1. Cobblestone Crossroad Path
                Area.Path roadNS = new Area.Path();
                roadNS.Name = "King's North-South Road";
                roadNS.FillChar = '.';
                roadNS.FillColor = Telnet.Color.Gray;
                roadNS.Points.Add(new Point(0, -8));
                roadNS.Points.Add(new Point(0, 8));
                roadNS.Parent = area;
                count++;

                Area.Path roadEW = new Area.Path();
                roadEW.Name = "Market Street";
                roadEW.FillChar = '.';
                roadEW.FillColor = Telnet.Color.Gray;
                roadEW.Points.Add(new Point(-8, 0));
                roadEW.Points.Add(new Point(8, 0));
                roadEW.Parent = area;
                count++;

                // 2. Town Square Safe Sanctum Region
                Area.Region sanctum = new Area.Region();
                sanctum.Name = "Haven Town Square";
                sanctum.FillChar = '+';
                sanctum.FillColor = Telnet.Color.BrightWhite;
                sanctum.LayerFlags.Set((int)Area.Layer.LayerFlag.NoAttack, true);
                sanctum.Points.Add(new Point(-2, -2));
                sanctum.Points.Add(new Point(2, -2));
                sanctum.Points.Add(new Point(2, 2));
                sanctum.Points.Add(new Point(-2, 2));
                sanctum.OnEnter = "actor:Send('{color:brightcyan}[Haven Sanctum] You step onto the peaceful cobblestones of Haven Town Square.{color:reset}')";
                sanctum.Parent = area;
                count++;

                // 3. Haven Armory & Potion Shop at (2, 0)
                Shop shop = new Shop();
                shop.Id = srv.NewGuid();
                shop.Name = "Haven Armory and Alchemy";
                shop.Description = "A bustling shop stocked with weapons, armor, backpacks, and healing draughts. Type 'list' to browse!";
                shop.Location = new Location(area, new Point(2, 0));
                shop.Parent = area;
                foreach (IIdentifiable itemObj in srv.DataSets[typeof(Item)].Values)
                {
                    shop.Items.Add(itemObj.Id);
                }
                count++;

                // 4. Spawn Resets for NPCs & Monsters
                AddNpcResetIfFound(area, srv.Get<Npc>("Captain Vane"), new Point(0, 0), ref count);
                AddNpcResetIfFound(area, srv.Get<Npc>("Guildmaster Kael"), new Point(-2, 0), ref count);
                AddNpcResetIfFound(area, srv.Get<Npc>("sewer rat"), new Point(0, -4), ref count);
                AddNpcResetIfFound(area, srv.Get<Npc>("goblin scout"), new Point(0, -6), ref count);
                AddNpcResetIfFound(area, srv.Get<Npc>("crypt skeleton"), new Point(4, -5), ref count);
                AddNpcResetIfFound(area, srv.Get<Npc>("crimson wyrmling"), new Point(0, -8), ref count);

                area.Update();
                this.Log("[Pack 7] Built Starter Town Map in '" + area.Name + "' (Roads, Safe Town Square, Armory Shop at (2,0), and NPC/Monster spawns).");
            }
            else
            {
                this.Log("[Pack 7] Starter Town objects already exist in '" + area.Name + "' — skipped duplicate placement.");
            }

            return count;
        }

        static void AddNpcResetIfFound(Area area, Npc npc, Point pt, ref int count)
        {
            if (area == null || npc == null)
                return;
            NpcReset reset = new NpcReset(new Location(area, pt));
            reset.ObjectId = npc.Id;
            reset.Count = 1;
            area.Resets.Add(reset);
            count++;
        }

        int InstallCommandsHelpChannelsSocials(Server srv)
        {
            int beforeCmds = Command.Commands != null ? Command.Commands.Count : 0;
            Command.Init(srv.ServerPath, srv.ServerPath);
            int afterCmds = Command.Commands != null ? Command.Commands.Count : 0;

            int beforeHelp = Help.HelpFiles != null ? Help.HelpFiles.Count : 0;
            Help.PostInit();
            int afterHelp = Help.HelpFiles != null ? Help.HelpFiles.Count : 0;

            Channel.PostInit();
            Social.PostInit();

            int delta = Math.Max(0, afterCmds - beforeCmds) + Math.Max(0, afterHelp - beforeHelp);
            this.Log("[Pack 8] Verified " + afterCmds + " Lua .cmd commands, " + afterHelp + " Help files, Channels, and Socials.");
            return delta;
        }
    }
}
