using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR;
using RVCRestructured.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimValiFFARW.Packs
{
    public class PackEditWindow : Window
    {
        public static readonly Color otherGrey = new Color(.15f, .15f, .15f, .10f);

        private const float CommonMargin = 5f;
        private const int ButtonOutlineWidth = 3;

        private readonly List<Pawn> curPackMates = new List<Pawn>();
        private readonly List<Pawn> oldPackMates = new List<Pawn>();
        private readonly List<Pawn> newPackMates = new List<Pawn>();

        private readonly Rect mainPart;
        private readonly Rect titlePart;
        private readonly Rect contentPart;
        private readonly Rect confirmationPart;
        private readonly Rect statusPart;

        private readonly Rect descriptionAndSelectionPart;
        private readonly Rect descriptionPart;
        private readonly Rect boniListPartOuter;
        private readonly Rect defSelectorButton;

        private readonly Pack pack;
        private readonly Map activeMap;

        private PackWorker newPackWorker;
        private PackDef newPackDef;

        private IEnumerable<Pawn> cachedAvailablePawns;

        private Vector2 memberScrollVector;
        private Vector2 descriptionScroll;
        private Vector2 boniScrollVector;
        private Rect memberListPartOuter;
        private Rect memberListPartInner;
        private Rect boniListPartInner;

        protected override float Margin => 0f;

        public override Vector2 InitialSize => new Vector2(600, 378);

        private bool CanAddMorePawnsToPack => curPackMates.Count + newPackMates.Count < (newPackDef?.maxSize ?? 0) && cachedAvailablePawns.Any();

        private List<FloatMenuOption> Options
        {
            get
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (PackDef def in DefDatabase<PackDef>.AllDefsListForReading.Where(def => !def.unique))
                {
                    FloatMenuOption option = new FloatMenuOption(def.LabelCap, () =>
                    {
                        newPackDef = def;
                        newPackWorker = def.GetNewPackWorker;
                        RefreshBoniInnerHeight();
                    });

                    options.Add(option);
                }

                return options;
            }
        }

        //TODO: When you add a member to a pack that has it's info displayed in a tab, you get an exception
        public PackEditWindow(Pack pack)
        {
            this.pack = pack;
            activeMap = pack.Members.First().Map;
            newPackDef = pack.Def;
            newPackWorker = pack.Worker;

            onlyOneOfTypeAllowed = true;
            forcePause = true;
            doCloseX = true;

            titlePart = new Rect(CommonMargin, 0f, InitialSize.x - CommonMargin * 2f, 30f);
            mainPart = new Rect(titlePart.x, titlePart.yMax + CommonMargin, titlePart.width, InitialSize.y - titlePart.yMax - CommonMargin * 3f);
            contentPart = new Rect(mainPart.x, mainPart.y, mainPart.width, 333f - CommonMargin * 3f);
            statusPart = new Rect(mainPart.x, contentPart.yMax + CommonMargin, mainPart.width, 20f);

            descriptionAndSelectionPart = new Rect(contentPart.x, contentPart.y, InitialSize.x * .7f + CommonMargin, 125f);
            defSelectorButton = new Rect(descriptionAndSelectionPart.x, descriptionAndSelectionPart.y, descriptionAndSelectionPart.width - CommonMargin, 30f);
            descriptionPart = new Rect(descriptionAndSelectionPart.x, defSelectorButton.yMax + CommonMargin, defSelectorButton.width, descriptionAndSelectionPart.height - CommonMargin - defSelectorButton.height);

            boniListPartOuter = new Rect(descriptionAndSelectionPart.xMax + CommonMargin, descriptionAndSelectionPart.y, InitialSize.x * .3f - CommonMargin * 4f, descriptionAndSelectionPart.height);
            memberListPartOuter = new Rect(descriptionAndSelectionPart.x, descriptionAndSelectionPart.yMax + CommonMargin * 2f, contentPart.width, PackInspectionWindow.ListRectTemplateHeight * 5);
            confirmationPart = new Rect(descriptionAndSelectionPart.x, memberListPartOuter.yMax + CommonMargin, contentPart.width, 30f);
            curPackMates.AddRange(pack.Members);
            RefreshBoniInnerHeight();
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle();
            DrawSeparators();
            DrawDefSelectorButton();
            DrawDescription();
            DrawBoniAndMemberList(out bool acceptPack, out string failreason0);
            DrawConfirmationButton(acceptPack, failreason0);
            DrawStatusbar();
        }

        private void DrawStatusbar()
        {
            if (newPackDef == null) return;
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(statusPart, "RVFFA_PackEditWindow_StatusBar".Translate(newPackDef.MinSizeToCreate, newPackDef.MinSizeToSustain));
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private void DrawConfirmationButton(bool acceptPack, string failreason0)
        {
            Widgets.DrawBoxSolidWithOutline(confirmationPart, otherGrey, Color.gray, 2);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(confirmationPart.MoveRect(new Vector2(CommonMargin, 0f)), "RVFFA_PackEditWindow_ConfirmChanges".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            IEnumerable<Pawn> fullCurrentTempPackMembers = curPackMates.Union(newPackMates);
            string failReason1 = null;
            bool isDefSwitched = pack.Def != newPackDef;

            int minimumNrOfMembers = (isDefSwitched ? newPackDef.MinSizeToCreate : newPackDef.MinSizeToSustain);
            int tempMemberCount = fullCurrentTempPackMembers.Count();

            bool enoughMembers = minimumNrOfMembers <= tempMemberCount;

            if (isDefSwitched)
            {
                acceptPack &= Pack.CanPawnsMakePack(newPackDef, fullCurrentTempPackMembers, true, out failReason1);
            } 
            else
            {
                acceptPack &= newPackMates.All(pawn => pack.Worker.PawnCanJoinPack(curPackMates, pawn, false, true, out failReason1));
            }

            if (!enoughMembers)
            {
                acceptPack = false;
                failReason1 = "RVFFA_PackWorker_CountLowerThanMin".Translate(tempMemberCount, minimumNrOfMembers);
            }

            if (!acceptPack)
            {
                Widgets.DrawBoxSolid(confirmationPart, new Color(1f, 0f, 0f, .2f));
            }

            if ((failreason0 ?? failReason1) is string anyReason) TooltipHandler.TipRegion(confirmationPart, anyReason);
            if (Widgets.ButtonInvisible(confirmationPart))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                if (acceptPack && isDefSwitched && Pack.TryMakeNewPackFromPawns(newPackDef, fullCurrentTempPackMembers, true, true, out Pack newPack))
                {
                    Packmanager manager = Packmanager.GetLastActivePackmanager;

                    Log.Message($"Switched from {pack.Def.label} to {newPackDef.label}");

                    manager.RemovePack(pack);
                    manager.AddPack(newPack);
                    PackInspectionWindow.GetCurrentPackInspectionWindow.OnOpen();
                    Close();
                } 
                else if(acceptPack && !isDefSwitched)
                {
                    Log.Message($"Added members {pack.NameColored}");
                    foreach (Pawn pawn in fullCurrentTempPackMembers) 
                    {
                        pack.AddMember(pawn, false);
                    }

                    foreach (Pawn pawn in oldPackMates)
                    {
                        pack.RemoveMember(pawn);
                    }

                    Close();
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
            }

            Widgets.DrawHighlightIfMouseover(confirmationPart);
        }

        private void DrawAddPawnButton()
        {
            if (!CanAddMorePawnsToPack) return;
            Vector2 tempPos = new Vector2(0f, PackInspectionWindow.ListRectTemplateHeight) * (newPackMates.Count + oldPackMates.Count + curPackMates.Count) + memberListPartInner.position;
            Rect temp = new Rect(tempPos, new Vector2(memberListPartInner.width, PackInspectionWindow.ListRectTemplateHeight));

            Widgets.DrawBoxSolidWithOutline(temp, otherGrey, Color.gray, 2);
            Widgets.DrawHighlightIfMouseover(temp);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(temp.MoveRect(new Vector2(CommonMargin, 0f)), "RVFFA_PackCreationWindow_AddAnotherPawn".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(temp))
            {
                List<FloatMenuOption> potentialPawnOptions = new List<FloatMenuOption>();
                foreach (Pawn pawn in cachedAvailablePawns)
                {
                    FloatMenuOption option = new FloatMenuOption(pawn.NameShortColored, () =>
                    {
                        if (pack.Members.Contains(pawn))
                        {
                            oldPackMates.Remove(pawn);
                            curPackMates.Add(pawn);
                        }
                        else
                            newPackMates.Add(pawn);


                        RefreshBoniInnerHeight();
                    }, pawn, Color.white);
                    potentialPawnOptions.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(potentialPawnOptions));
            }
        }

        private bool DrawOpinionBar(Pawn otherMember, IEnumerable<Pawn> otherMembers, PackDef def, Rect rect, int locationModif, OpinionBarType type, out string reason)
        {
            bool nothingIllegal = true;
            int opinion = Convert.ToInt32(newPackWorker.EvaluateAverageOpinionForPawn(otherMember, otherMembers));

            Texture2D barTex = PackInspectionWindow.GetBarTexForOpinion(opinion, def);
            Vector2 tempPos = new Vector2(0f, PackInspectionWindow.ListRectTemplateHeight) * locationModif + rect.position;
            Rect tempRect = new Rect(tempPos, new Vector2(rect.width, PackInspectionWindow.ListRectTemplateHeight));

            Rect tempMemberName = tempRect.LeftPartPixels(100f).MoveRect(new Vector2(CommonMargin, CommonMargin));
            Rect tempOpinionPre = tempRect.RightPartPixels(tempRect.height * 2f);
            Rect tempOpinion = tempOpinionPre.ContractedBy(CommonMargin);
            Rect tempBar = new Rect(tempMemberName.xMax + CommonMargin, tempRect.y, tempRect.width - tempMemberName.width - CommonMargin * 2 - tempOpinionPre.width, PackInspectionWindow.ListRectTemplateHeight).ContractedBy(CommonMargin);
            Rect tempCloseButton = tempRect.RightPartPixels(tempRect.height).ContractedBy(CommonMargin);

            if (type == OpinionBarType.oldMember)
            {
                GUI.color = Color.gray;
            }
            tempRect.DoRectHighlight(locationModif % 2 == 0);
            MouseoverSounds.DoRegion(tempRect);
            Widgets.DrawHighlightIfMouseover(tempRect);
            if (Widgets.ButtonInvisible(tempRect.LeftPartPixels(200f))) Find.WindowStack.Add(new Dialog_InfoCard(otherMember));
            if (!newPackWorker.PawnCanJoinPack(otherMembers, otherMember, true, true, out reason) && type == OpinionBarType.newMember)
            {
                nothingIllegal = false;
                Widgets.DrawBoxSolid(tempRect, new Color(1f, 0f, 0f, .2f));
                TooltipHandler.TipRegion(tempRect, reason);
            }

            Widgets.Label(tempMemberName, otherMember.NameShortColored);
            Widgets.Label(tempOpinion, opinion.ToString());

            if (type != OpinionBarType.oldMember)
            {
                Widgets.FillableBar(tempBar, (100f + opinion) / 200f, barTex);
            }
            else
            {
                Text.Font = GameFont.Tiny;
                Widgets.Label(tempBar, "RVFFA_PackEditWindow_PawnToBeRemoved".Translate(otherMember.LabelShortCap));
                Text.Font = GameFont.Small;

                Widgets.DrawBoxSolid(tempRect, new Color(otherGrey.r, otherGrey.g, otherGrey.b, .2f));
            }

            Widgets.DrawTextureFitted(tempCloseButton, TexButton.DeleteX, 1f);
            if (Widgets.ButtonInvisible(tempCloseButton))
            {
                switch (type)
                {
                    case OpinionBarType.curMember:
                        curPackMates.Remove(otherMember);
                        oldPackMates.Add(otherMember);
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                        break;
                    case OpinionBarType.oldMember:
                        SoundDefOf.ClickReject.PlayOneShotOnCamera();
                        break;
                    case OpinionBarType.newMember:
                        newPackMates.Remove(otherMember);
                        SoundDefOf.TabClose.PlayOneShotOnCamera();
                        break;
                }

                RefreshBoniInnerHeight();
            }

            GUI.color = Color.white;
            return nothingIllegal;
        }

        private void DrawBoniAndMemberList(out bool acceptPack, out string failreason0)
        {
            acceptPack = true;
            failreason0 = null;

            if (newPackDef != null)
            {
                int total = 0;
                IEnumerable<Pawn> union = curPackMates.Union(newPackMates);

                PackInspectionWindow.DrawBoniList(boniListPartOuter, boniListPartInner, newPackDef, ref boniScrollVector);
                Widgets.BeginScrollView(memberListPartOuter, ref memberScrollVector, memberListPartInner);

                for (int i = 0; i < curPackMates.Count; i++)
                {
                    Pawn pawn = curPackMates[i];
                    DrawOpinionBar(pawn, union, newPackDef, memberListPartInner, total++, OpinionBarType.curMember, out _);
                }

                for (int i = 0; i < newPackMates.Count; i++)
                {
                    Pawn pawn = newPackMates[i];
                    acceptPack &= DrawOpinionBar(pawn, union, newPackDef, memberListPartInner, total++, OpinionBarType.newMember, out failreason0);
                }

                for (int i = 0; i < oldPackMates.Count; i++)
                {
                    Pawn pawn = oldPackMates[i];
                    DrawOpinionBar(pawn, union, newPackDef, memberListPartInner, total++, OpinionBarType.oldMember, out _);
                }

                DrawAddPawnButton();
                Widgets.EndScrollView();
            }
        }

        private void DrawDescription()
        {
            Widgets.LabelScrollable(descriptionPart, newPackDef?.description ?? string.Empty, ref descriptionScroll);
        }

        private void DrawSeparators()
        {
            GUI.color = Color.gray;
            Widgets.DrawLineVertical(descriptionAndSelectionPart.xMax, descriptionAndSelectionPart.y, descriptionAndSelectionPart.height);

            Widgets.DrawLineHorizontal(titlePart.x, titlePart.yMax, titlePart.width);
            Widgets.DrawLineHorizontal(descriptionAndSelectionPart.x, descriptionAndSelectionPart.yMax + CommonMargin, contentPart.width);
            Widgets.DrawLineHorizontal(contentPart.x, contentPart.yMax + CommonMargin, contentPart.width);
            GUI.color = Color.white;
        }

        private void DrawDefSelectorButton()
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.DrawBoxSolidWithOutline(defSelectorButton, otherGrey, Color.gray, ButtonOutlineWidth);
            Widgets.DrawHighlightIfMouseover(defSelectorButton);
            Widgets.Label(defSelectorButton.MoveRect(new Vector2(CommonMargin + ButtonOutlineWidth, 0f)), newPackDef?.LabelCap ?? $"{"RVFFA_PackCreationWindow_SelectPackDef".Translate()}");
            Widgets.DrawTextureFitted(defSelectorButton.RightPartPixels(defSelectorButton.height).ContractedBy(3.5f).Rounded(), TexButton.Collapse, 1f);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(defSelectorButton)) Find.WindowStack.Add(new FloatMenu(Options));
            TooltipHandler.TipRegion(defSelectorButton, "RVFFA_PackCreationWindow_SelectPackDefButton".Translate());
        }

        private void DrawTitle()
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(titlePart, $"<b>{"RVFFA_PackEditWindow_Title".Translate(pack.NameColored)}</b>");
            Text.Font = GameFont.Small;
        }

        private void RefreshBoniInnerHeight()
        {
            cachedAvailablePawns = activeMap.mapPawns.FreeColonistsSpawned.Where(pawn => !curPackMates.Contains(pawn) && !newPackMates.Contains(pawn));
            boniListPartInner = boniListPartOuter.GetInnerScrollRect(PackInspectionWindow.ListRectTemplateHeight * newPackDef?.memberHediffs.Count ?? 0);
            memberListPartInner = memberListPartOuter.GetInnerScrollRect(PackInspectionWindow.ListRectTemplateHeight * (oldPackMates.Count + newPackMates.Count + (CanAddMorePawnsToPack ? 1 : 0)));
        }


        private enum OpinionBarType
        {
            curMember,
            oldMember,
            newMember
        }
    }
}
