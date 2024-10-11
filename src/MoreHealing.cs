﻿using System.Collections.Generic;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Modding;
using MoreHealing.Consts;
using SFCore;
using SFCore.Generics;
using SFCore.Utils;
using UnityEngine;

namespace MoreHealing;

class MoreHealing : SaveSettingsMod<MhSettings>
{

    public TextureStrings Ts { get; private set; }
    public List<int> CharmIDs { get; private set; }

    public override string GetVersion() => SFCore.Utils.Util.GetVersion(Assembly.GetExecutingAssembly());

    public MoreHealing() : base("More Healing")
    {
        Ts = new TextureStrings();
    }

    public override void Initialize()
    {
        Log("Initializing");

        CharmIDs = CharmHelper.AddSprites(Ts.Get(TextureStrings.QuickerFocusKey), Ts.Get(TextureStrings.QuickestFocusKey), Ts.Get(TextureStrings.DeeperFocusKey), Ts.Get(TextureStrings.DeepestFocusKey));

        InitCallbacks();

        Log("Initialized");
    }

    private void InitCallbacks()
    {
        ModHooks.GetPlayerBoolHook += OnGetPlayerBoolHook;
        ModHooks.SetPlayerBoolHook += OnSetPlayerBoolHook;
        ModHooks.GetPlayerIntHook += OnGetPlayerIntHook;
        ModHooks.LanguageGetHook += OnLanguageGetHook;

        On.HeroController.Start += OnHeroControllerStart;
    }

    private void OnHeroControllerStart(On.HeroController.orig_Start orig, HeroController self)
    {
        orig(self);

        AddCharmStates(self);
    }

    private void AddCharmStates(HeroController self)
    {
        var spellFsm = self.gameObject.LocateMyFSM("Spell Control");

        #region Quick Focus Speeds

        spellFsm.GetState("Set Focus Speed").SaveActions();

        spellFsm.CopyState("Set Focus Speed", "Set QuickerFocus Speed");
        spellFsm.RemoveAction("Set QuickerFocus Speed", 0);
        spellFsm.RemoveAction("Set QuickerFocus Speed", 0);
        spellFsm.RemoveAction("Set QuickerFocus Speed", 2);
        spellFsm.GetAction<PlayerDataBoolTest>("Set QuickerFocus Speed", 0).boolName = $"equippedCharm_{CharmIDs[0]}";
        spellFsm.AddAction("Set QuickerFocus Speed", new FloatMultiply
        {
            floatVariable = spellFsm.GetFloatVariable("Time Per MP Drain"),
            multiplyBy = 2f / 3f
        });
        spellFsm.GetAction<FloatMultiply>("Set QuickerFocus Speed", 2).multiplyBy = Mathf.Pow(2f/3f, 2);
        spellFsm.GetState("Set QuickerFocus Speed").SaveActions();

        spellFsm.CopyState("Set QuickerFocus Speed", "Set QuickestFocus Speed");
        spellFsm.GetAction<PlayerDataBoolTest>("Set QuickestFocus Speed", 0).boolName = $"equippedCharm_{CharmIDs[1]}";
        spellFsm.GetAction<FloatMultiply>("Set QuickestFocus Speed", 2).multiplyBy = Mathf.Pow(2f/3f, 3);
        spellFsm.GetState("Set QuickestFocus Speed").SaveActions();

        spellFsm.ChangeTransition("Set Focus Speed", FsmEvent.Finished.Name, "Set QuickerFocus Speed");
        spellFsm.ChangeTransition("Set QuickerFocus Speed", FsmEvent.Finished.Name, "Set QuickestFocus Speed");

        #endregion

        #region Deep Focus Speeds

        spellFsm.GetState("Deep Focus Speed").SaveActions();

        spellFsm.CopyState("Deep Focus Speed", "Deeper Focus Speed");
        spellFsm.GetAction<PlayerDataBoolTest>("Deeper Focus Speed", 0).boolName = $"equippedCharm_{CharmIDs[2]}";
        spellFsm.GetAction<FloatMultiply>("Deeper Focus Speed", 1).multiplyBy = Mathf.Pow(1.65f, 2);
        spellFsm.GetState("Deeper Focus Speed").SaveActions();

        spellFsm.CopyState("Deeper Focus Speed", "Deepest Focus Speed");
        spellFsm.GetAction<PlayerDataBoolTest>("Deepest Focus Speed", 0).boolName = $"equippedCharm_{CharmIDs[3]}";
        spellFsm.GetAction<FloatMultiply>("Deepest Focus Speed", 1).multiplyBy = Mathf.Pow(1.65f, 3);
        spellFsm.GetState("Deepest Focus Speed").SaveActions();

        spellFsm.ChangeTransition("Deep Focus Speed", FsmEvent.Finished.Name, "Deeper Focus Speed");
        spellFsm.ChangeTransition("Deeper Focus Speed", FsmEvent.Finished.Name, "Deepest Focus Speed");

        #endregion

        #region Hp Amounts

        spellFsm.GetState("Set HP Amount").SaveActions();

        spellFsm.CopyState("Set HP Amount", "Set HP Amount Deeper");
        spellFsm.RemoveAction("Set HP Amount Deeper", 0);
        spellFsm.RemoveAction("Set HP Amount Deeper", 1);
        spellFsm.GetAction<PlayerDataBoolTest>("Set HP Amount Deeper", 0).boolName = $"equippedCharm_{CharmIDs[2]}";
        spellFsm.AddAction("Set HP Amount Deeper", new IntAdd
        {
            intVariable = spellFsm.GetIntVariable("Health Increase"),
            add = 2
        });
        spellFsm.GetState("Set HP Amount Deeper").SaveActions();

        spellFsm.CopyState("Set HP Amount Deeper", "Set HP Amount Deepest");
        spellFsm.GetAction<PlayerDataBoolTest>("Set HP Amount Deepest", 0).boolName = $"equippedCharm_{CharmIDs[3]}";
        spellFsm.GetAction<IntAdd>("Set HP Amount Deepest", 1).add = 3;
        spellFsm.GetState("Set HP Amount Deepest").SaveActions();

        spellFsm.CopyState("Set HP Amount Deeper", "Set HP Amount 2 Deeper");
        spellFsm.GetState("Set HP Amount 2 Deeper").SaveActions();

        spellFsm.CopyState("Set HP Amount 2 Deeper", "Set HP Amount 2 Deepest");
        spellFsm.GetAction<PlayerDataBoolTest>("Set HP Amount 2 Deepest", 0).boolName = $"equippedCharm_{CharmIDs[3]}";
        spellFsm.GetAction<IntAdd>("Set HP Amount 2 Deepest", 1).add = 3;
        spellFsm.GetState("Set HP Amount 2 Deepest").SaveActions();

        spellFsm.ChangeTransition("Set HP Amount", FsmEvent.Finished.Name, "Set HP Amount Deeper");
        spellFsm.ChangeTransition("Set HP Amount Deeper", FsmEvent.Finished.Name, "Set HP Amount Deepest");
        spellFsm.ChangeTransition("Set HP Amount Deepest", FsmEvent.Finished.Name, "Focus Heal");
        spellFsm.ChangeTransition("Set HP Amount 2", FsmEvent.Finished.Name, "Set HP Amount 2 Deeper");
        spellFsm.ChangeTransition("Set HP Amount 2 Deeper", FsmEvent.Finished.Name, "Set HP Amount 2 Deepest");
        spellFsm.ChangeTransition("Set HP Amount 2 Deepest", FsmEvent.Finished.Name, "Focus Heal 2");

        #endregion

        #region shape of unn

        FloatAdd floatAdd = new FloatAdd();
        spellFsm.GetAction<PlayerDataBoolTest>("Slug?", 0).isFalse = null;
        spellFsm.AddAction("Slug?", spellFsm.GetAction<PlayerDataBoolTest>("Slug?", 0));
        spellFsm.GetAction<PlayerDataBoolTest>("Slug?", 1).boolName = $"equippedCharm_{CharmIDs[0]}";
        spellFsm.AddAction("Slug?", spellFsm.GetAction<PlayerDataBoolTest>("Slug?", 0));
        spellFsm.GetAction<PlayerDataBoolTest>("Slug?", 2).boolName = $"equippedCharm_{CharmIDs[1]}";
        spellFsm.AddMethod("Slug?", () =>
        {
            var slugSpeedL = spellFsm.GetFloatVariable("Slug Speed L").Value;
            var slugSpeedR = spellFsm.GetFloatVariable("Slug Speed R").Value;
            Log($"Slug Speed L set to {slugSpeedL}");
            Log($"Slug Speed R set to {slugSpeedR}");
        });
        spellFsm.GetState("Slug?").SaveActions();

        spellFsm.AddState("Slug Speed Quicker");
        spellFsm.AddAction("Slug Speed Quicker", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_{CharmIDs[0]}",
            isTrue = null,
            isFalse = FsmEvent.Finished
        });
        spellFsm.AddAction("Slug Speed Quicker", new FloatAdd
        {
            floatVariable = spellFsm.GetFloatVariable("Slug Speed L"),
            add = -6f
        });
        spellFsm.AddAction("Slug Speed Quicker", new FloatAdd
        {
            floatVariable = spellFsm.GetFloatVariable("Slug Speed R"),
            add = 6f
        });
        spellFsm.AddMethod("Slug Speed Quicker", () =>
        {
            var slugSpeedL = spellFsm.GetFloatVariable("Slug Speed L").Value;
            var slugSpeedR = spellFsm.GetFloatVariable("Slug Speed R").Value;
            Log($"Slug Speed L set to {slugSpeedL}");
            Log($"Slug Speed R set to {slugSpeedR}");
        });
        spellFsm.GetState("Slug Speed Quicker").SaveActions();

        spellFsm.CopyState("Slug Speed Quicker", "Slug Speed Quickest");
        spellFsm.GetAction<PlayerDataBoolTest>("Slug Speed Quickest", 0).boolName = $"equippedCharm_{CharmIDs[1]}";
        spellFsm.RemoveAction("Slug Speed Quickest", 3);
        spellFsm.AddMethod("Slug Speed Quickest", () =>
        {
            var slugSpeedL = spellFsm.GetFloatVariable("Slug Speed L").Value;
            var slugSpeedR = spellFsm.GetFloatVariable("Slug Speed R").Value;
            Log($"Slug Speed L set to {slugSpeedL}");
            Log($"Slug Speed R set to {slugSpeedR}");
        });
        spellFsm.GetState("Slug Speed Quickest").SaveActions();

        spellFsm.ChangeTransition("Slug Speed", FsmEvent.Finished.Name, "Slug Speed Quicker");
        spellFsm.AddTransition("Slug Speed Quicker", FsmEvent.Finished.Name, "Slug Speed Quickest");
        spellFsm.ChangeTransition("Slug Speed Quicker", FsmEvent.Finished.Name, "Slug Speed Quickest");
        spellFsm.AddTransition("Slug Speed Quickest", FsmEvent.Finished.Name, "Anim Check");
        spellFsm.ChangeTransition("Slug Speed Quickest", FsmEvent.Finished.Name, "Anim Check");

        #endregion

        #region spore shroom

        GameObject sporeCloudGo = spellFsm.GetAction<SpawnObjectFromGlobalPool>("Spore Cloud", 3).gameObject.Value;
        GameObject dungCloudGo = spellFsm.GetAction<SpawnObjectFromGlobalPool>("Dung Cloud", 0).gameObject.Value;

        var sporeCloudFsm = sporeCloudGo.LocateMyFSM("Control");
        if (sporeCloudFsm.FsmStates[0].Fsm == null)
        {
            sporeCloudFsm.Preprocess();
        }
        var sporeCloudDeepEvent = sporeCloudFsm.GetTransition("Init", "DEEP").FsmEvent;
        sporeCloudFsm.GetAction<PlayerDataBoolTest>("Init", 2).isFalse = null;
        sporeCloudFsm.AddAction("Init", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_{CharmIDs[2]}",
            isTrue = sporeCloudDeepEvent,
            isFalse = null
        });
        sporeCloudFsm.AddAction("Init", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_{CharmIDs[3]}",
            isTrue = sporeCloudDeepEvent,
            isFalse = null
        });
        sporeCloudFsm.GetState("Init").SaveActions();

        FsmFloat sporeCloudXScale = sporeCloudFsm.GetFloatVariable("X Scale");
        FsmFloat sporeCloudYScale = sporeCloudFsm.GetFloatVariable("Y Scale");
        sporeCloudXScale.Value = 1f;
        sporeCloudYScale.Value = 1f;

        sporeCloudFsm.RemoveAction("Deep", 1);
        sporeCloudFsm.AddAction("Deep", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_34",
            isFalse = FsmEvent.Finished
        });
        sporeCloudFsm.AddAction("Deep", new FloatMultiply
        {
            floatVariable = sporeCloudXScale,
            multiplyBy = 1.35f
        });
        sporeCloudFsm.AddAction("Deep", new FloatMultiply
        {
            floatVariable = sporeCloudYScale,
            multiplyBy = 1.35f
        });
        sporeCloudFsm.GetState("Deep").SaveActions();

        sporeCloudFsm.CopyState("Deep", "Deeper");
        sporeCloudFsm.RemoveAction("Deeper", 0);
        sporeCloudFsm.GetAction<PlayerDataBoolTest>("Deeper", 0).boolName = $"equippedCharm_{CharmIDs[2]}";
        sporeCloudFsm.GetAction<FloatMultiply>("Deeper", 1).multiplyBy = Mathf.Pow(1.35f, 2f);
        sporeCloudFsm.GetAction<FloatMultiply>("Deeper", 2).multiplyBy = Mathf.Pow(1.35f, 2f);
        sporeCloudFsm.GetState("Deeper").SaveActions();

        sporeCloudFsm.CopyState("Deeper", "Deepest");
        sporeCloudFsm.GetAction<PlayerDataBoolTest>("Deepest", 0).boolName = $"equippedCharm_{CharmIDs[3]}";
        sporeCloudFsm.GetAction<FloatMultiply>("Deepest", 1).multiplyBy = Mathf.Pow(1.35f, 3f);
        sporeCloudFsm.GetAction<FloatMultiply>("Deepest", 2).multiplyBy = Mathf.Pow(1.35f, 3f);
        sporeCloudFsm.GetState("Deepest").SaveActions();

        sporeCloudFsm.AddState("Apply Scale");
        sporeCloudFsm.AddAction("Apply Scale", new SetScale
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.UseOwner
            },
            x = sporeCloudXScale,
            y = sporeCloudYScale
        });
        sporeCloudFsm.GetState("Apply Scale").SaveActions();

        sporeCloudFsm.ChangeTransition("Deep", FsmEvent.Finished.Name, "Deeper");
        sporeCloudFsm.ChangeTransition("Deeper", FsmEvent.Finished.Name, "Deepest");
        sporeCloudFsm.ChangeTransition("Deepest", FsmEvent.Finished.Name, "Apply Scale");
        sporeCloudFsm.AddTransition("Apply Scale", FsmEvent.Finished.Name, "Wait");

        var dungCloudFsm = dungCloudGo.LocateMyFSM("Control");
        if (dungCloudFsm.FsmStates[0].Fsm == null)
        {
            dungCloudFsm.Preprocess();
        }
        var dungCloudDeepEvent = dungCloudFsm.GetTransition("Init", "DEEP").FsmEvent;
        dungCloudFsm.GetAction<PlayerDataBoolTest>("Init", 2).isFalse = null;
        dungCloudFsm.AddAction("Init", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_{CharmIDs[2]}",
            isTrue = dungCloudDeepEvent,
            isFalse = null
        });
        dungCloudFsm.AddAction("Init", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_{CharmIDs[3]}",
            isTrue = dungCloudDeepEvent,
            isFalse = null
        });
        dungCloudFsm.GetState("Init").SaveActions();

        FsmFloat dungCloudXScale = dungCloudFsm.GetFloatVariable("X Scale");
        FsmFloat dungCloudYScale = dungCloudFsm.GetFloatVariable("Y Scale");
        dungCloudXScale.Value = 1f;
        dungCloudYScale.Value = 1f;

        dungCloudFsm.RemoveAction("Deep", 1);
        dungCloudFsm.AddAction("Deep", new PlayerDataBoolTest
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.SpecifyGameObject,
                GameObject = GameManager.instance.gameObject
            },
            boolName = $"equippedCharm_34",
            isFalse = FsmEvent.Finished
        });
        dungCloudFsm.AddAction("Deep", new FloatMultiply
        {
            floatVariable = dungCloudXScale,
            multiplyBy = 1.35f
        });
        dungCloudFsm.AddAction("Deep", new FloatMultiply
        {
            floatVariable = dungCloudYScale,
            multiplyBy = 1.35f
        });
        dungCloudFsm.GetState("Deep").SaveActions();

        dungCloudFsm.CopyState("Deep", "Deeper");
        dungCloudFsm.RemoveAction("Deeper", 0);
        dungCloudFsm.GetAction<PlayerDataBoolTest>("Deeper", 0).boolName = $"equippedCharm_{CharmIDs[2]}";
        dungCloudFsm.GetAction<FloatMultiply>("Deeper", 1).multiplyBy = Mathf.Pow(1.35f, 2f);
        dungCloudFsm.GetAction<FloatMultiply>("Deeper", 2).multiplyBy = Mathf.Pow(1.35f, 2f);
        dungCloudFsm.GetState("Deeper").SaveActions();

        dungCloudFsm.CopyState("Deeper", "Deepest");
        dungCloudFsm.GetAction<PlayerDataBoolTest>("Deepest", 0).boolName = $"equippedCharm_{CharmIDs[3]}";
        dungCloudFsm.GetAction<FloatMultiply>("Deepest", 1).multiplyBy = Mathf.Pow(1.35f, 3f);
        dungCloudFsm.GetAction<FloatMultiply>("Deepest", 2).multiplyBy = Mathf.Pow(1.35f, 3f);
        dungCloudFsm.GetState("Deepest").SaveActions();

        dungCloudFsm.AddState("Apply Scale");
        dungCloudFsm.AddAction("Apply Scale", new SetScale
        {
            gameObject = new FsmOwnerDefault
            {
                OwnerOption = OwnerDefaultOption.UseOwner
            },
            x = dungCloudXScale,
            y = dungCloudYScale
        });
        dungCloudFsm.GetState("Apply Scale").SaveActions();

        dungCloudFsm.ChangeTransition("Deep", FsmEvent.Finished.Name, "Deeper");
        dungCloudFsm.ChangeTransition("Deeper", FsmEvent.Finished.Name, "Deepest");
        dungCloudFsm.ChangeTransition("Deepest", FsmEvent.Finished.Name, "Apply Scale");
        dungCloudFsm.AddTransition("Apply Scale", FsmEvent.Finished.Name, "Wait");

        #endregion

        spellFsm.Preprocess();
        sporeCloudFsm.Preprocess();
        dungCloudFsm.Preprocess();

        spellFsm.Log();
        sporeCloudFsm.Log();
        dungCloudFsm.Log();
        spellFsm.MakeLog();
        sporeCloudFsm.MakeLog();
        dungCloudFsm.MakeLog();

        spellFsm.Preprocess();
        sporeCloudFsm.Preprocess();
        dungCloudFsm.Preprocess();
    }

    #region ModHooks

    private string[] _charmNames =
    {
        "Quicker Focus",
        "Quickest Focus",
        "Deeper Focus",
        "Deepest Focus",
    };
    private string[] _charmDescriptions =
    {
        "A dense charm containing a crystal lens.<br><br>Increases the speed of focusing SOUL, allowing the bearer to heal damage even faster.",
        "A very dense charm containing a crystal lens.<br><br>Increases the speed of focusing SOUL, allowing the bearer to heal damage faster than nothing else.",
        "Naturally formed within a crystal over a longer period. Draws in SOUL from the surrounding air.<br><br>The bearer will focus SOUL at a slower rate, but the healing effect will triple.",
        "Naturally formed within a crystal over the longest period. Draws in SOUL from the surrounding air.<br><br>The bearer will focus SOUL at a slower rate, but the healing effect will quadruple.",
    };
    private string OnLanguageGetHook(string key, string sheet, string orig)
    {
        if (key.StartsWith("CHARM_NAME_"))
        {
            int charmNum = int.Parse(key.Split('_')[2]);
            if (CharmIDs.Contains(charmNum))
            {
                return _charmNames[CharmIDs.IndexOf(charmNum)];
            }
        }
        else if (key.StartsWith("CHARM_DESC_"))
        {
            int charmNum = int.Parse(key.Split('_')[2]);
            if (CharmIDs.Contains(charmNum))
            {
                return _charmDescriptions[CharmIDs.IndexOf(charmNum)];
            }
        }
        return orig;
    }
    private bool OnGetPlayerBoolHook(string target, bool orig)
    {
        if (target.StartsWith("gotCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                return SaveSettings.gotCharms[CharmIDs.IndexOf(charmNum)];
            }
        }
        if (target.StartsWith("newCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                return SaveSettings.newCharms[CharmIDs.IndexOf(charmNum)];
            }
        }
        if (target.StartsWith("equippedCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                return SaveSettings.equippedCharms[CharmIDs.IndexOf(charmNum)];
            }
        }
        return orig;
    }
    private bool OnSetPlayerBoolHook(string target, bool orig)
    {
        if (target.StartsWith("gotCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                SaveSettings.gotCharms[CharmIDs.IndexOf(charmNum)] = orig;
                return orig;
            }
        }
        if (target.StartsWith("newCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                SaveSettings.newCharms[CharmIDs.IndexOf(charmNum)] = orig;
                return orig;
            }
        }
        if (target.StartsWith("equippedCharm_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                SaveSettings.equippedCharms[CharmIDs.IndexOf(charmNum)] = orig;
                return orig;
            }
        }
        return orig;
    }
    private int OnGetPlayerIntHook(string target, int orig)
    {
        if (target.StartsWith("charmCost_"))
        {
            int charmNum = int.Parse(target.Split('_')[1]);
            if (CharmIDs.Contains(charmNum))
            {
                return SaveSettings.charmCosts[CharmIDs.IndexOf(charmNum)];
            }
        }
        return orig;
    }

    #endregion
}