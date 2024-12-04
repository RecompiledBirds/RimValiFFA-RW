using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.Windows;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimValiFFARW.Packs
{
    public class PackCreationWindow : Window
    {
        public static readonly Color otherGrey = new Color(.15f, .15f, .15f, .10f);

        private const float CommonMargin = 5f;
        private const int ButtonOutlineWidth = 3;

        private readonly List<Pawn> fullTempPackMembers = new List<Pawn>();
        private readonly List<Pawn> potentialMembers = new List<Pawn>();

        private readonly Rect mainPart;
        private readonly Rect titlePart;
        private readonly Rect contentPart;
        private readonly Rect confirmationPart;
        private readonly Rect statusPart;

        private readonly Rect descriptionAndSelectionPart;
        private readonly Rect descriptionPart;
        private readonly Rect boniListPartOuter;
        private readonly Rect defSelectorButton;

        private readonly Pawn firstPawn;
        [AllowNull]
        private PackWorker packWorker;
        [AllowNull]
        private PackDef packDef;
        [AllowNull]
        private IEnumerable<Pawn> cachedAvailablePawns;

        private Vector2 memberScrollVector;
        private Vector2 descriptionScroll;
        private Vector2 boniScrollVector;
        private Rect memberListPartOuter;
        private Rect memberListPartInner;
        private Rect boniListPartInner;

        protected override float Margin => 0f;
        public override Vector2 InitialSize => new Vector2(600, 388);

        public PackCreationWindow(Pawn firstPawn)
        {
            this.firstPawn = firstPawn;

            onlyOneOfTypeAllowed = true;
            forcePause = true;
            doCloseX = true;

            titlePart = new Rect(CommonMargin, 0f, InitialSize.x - CommonMargin * 2f, 30f);
            mainPart = new Rect(titlePart.x, titlePart.yMax + CommonMargin, titlePart.width, InitialSize.y - titlePart.yMax - CommonMargin * 3f);
            contentPart = new Rect(mainPart.x, mainPart.y, mainPart.width, 333f - CommonMargin * 3f);
            statusPart = new Rect(mainPart.x, contentPart.yMax + CommonMargin * 2f, mainPart.width, 20f);

            descriptionAndSelectionPart = new Rect(contentPart.x, contentPart.y, InitialSize.x * .6f + CommonMargin, 125f);
            defSelectorButton = new Rect(descriptionAndSelectionPart.x, descriptionAndSelectionPart.y, descriptionAndSelectionPart.width - CommonMargin, 30f);
            descriptionPart = new Rect(descriptionAndSelectionPart.x, defSelectorButton.yMax + CommonMargin, defSelectorButton.width, descriptionAndSelectionPart.height - CommonMargin - defSelectorButton.height);

            boniListPartOuter = new Rect(descriptionAndSelectionPart.xMax + CommonMargin, descriptionAndSelectionPart.y, InitialSize.x * .4f - CommonMargin * 4f, descriptionAndSelectionPart.height);
            memberListPartOuter = new Rect(descriptionAndSelectionPart.x, descriptionAndSelectionPart.yMax + CommonMargin * 2f, contentPart.width, PackInspectionWindow.ListRectTemplateHeight * 5);
            confirmationPart = new Rect(descriptionAndSelectionPart.x, memberListPartOuter.yMax + CommonMargin, contentPart.width, 30f);
            fullTempPackMembers.Add(firstPawn);
            RefreshBoniInnerHeight();
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawSeparators();
            DrawTitle();
            DrawDefSelectorButton();
            DrawDescription();
            DrawBoniAndMemberList(out bool acceptPack, out string? failreason0);
            DrawConfirmationButton(acceptPack, failreason0);
            DrawStatusbar();
        }

        private void DrawConfirmationButton(bool acceptPack, string? failreason0)
        {
            Widgets.DrawBoxSolidWithOutline(confirmationPart, otherGrey, Color.gray, 2);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(confirmationPart.MoveRect(new Vector2(CommonMargin, 0f)), "RVFFA_PackCreationWindow_Confirmation".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            acceptPack &= Pack.CanPawnsMakePack(packDef, fullTempPackMembers, true, out string? failReason1);
            if (!acceptPack)
            {
                Widgets.DrawBoxSolid(confirmationPart, new Color(1f, 0f, 0f, .2f));
            }

            if ((failreason0 ?? failReason1) is string anyReason) TooltipHandler.TipRegion(confirmationPart, anyReason);
            if (Widgets.ButtonInvisible(confirmationPart))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                if (acceptPack && Pack.TryMakeNewPackFromPawns(packDef, fullTempPackMembers, false, true, out Pack? pack))
                {
                    Packmanager.GetLastActivePackmanager.AddPack(pack);
                    PackInspectionWindow.GetCurrentPackInspectionWindow.OnOpen();
                    Close();
                }
                else
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
            }

            Widgets.DrawHighlightIfMouseover(confirmationPart);
        }

        private void DrawBoniAndMemberList(out bool acceptPack, out string? failreason0)
        {
            acceptPack = true;
            failreason0 = null;

            if (packDef != null)
            {
                PackInspectionWindow.DrawBoniList(boniListPartOuter, boniListPartInner, packDef, ref boniScrollVector);
                Widgets.BeginScrollView(memberListPartOuter, ref memberScrollVector, memberListPartInner);

                for (int i = 0; i < potentialMembers.Count; i++)
                {
                    Pawn pawn = potentialMembers[i];
                    acceptPack &= DrawOpinionBar(pawn, fullTempPackMembers, packDef, memberListPartInner, i, out failreason0);
                }

                DrawAddPawnButton();
                Widgets.EndScrollView();
            }
        }

        private void DrawDescription()
        {
            Widgets.LabelScrollable(descriptionPart, packDef?.description ?? string.Empty, ref descriptionScroll);
        }

        private void DrawDefSelectorButton()
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.DrawBoxSolidWithOutline(defSelectorButton, otherGrey, Color.gray, ButtonOutlineWidth);
            Widgets.DrawHighlightIfMouseover(defSelectorButton);
            Widgets.Label(defSelectorButton.MoveRect(new Vector2(CommonMargin + ButtonOutlineWidth, 0f)), packDef?.LabelCap ?? $"{"RVFFA_PackCreationWindow_SelectPackDef".Translate()}");
            Widgets.DrawTextureFitted(defSelectorButton.RightPartPixels(defSelectorButton.height).ContractedBy(3.5f).Rounded(), TexButton.Collapse, 1f);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            if (Widgets.ButtonInvisible(defSelectorButton)) Find.WindowStack.Add(new FloatMenu(Options));
            TooltipHandler.TipRegion(defSelectorButton, "RVFFA_PackCreationWindow_SelectPackDefButton".Translate());
        }

        private void DrawTitle()
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(titlePart, $"<b>{"RVFFA_PackCreationWindow_Title".Translate()}</b>");
            Text.Font = GameFont.Small;
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

        private void DrawAddPawnButton()
        {
            if (!CanAddMorePawnsToPack) return;
            Vector2 tempPos = new Vector2(0f, PackInspectionWindow.ListRectTemplateHeight) * potentialMembers.Count + memberListPartInner.position;
            Rect temp = new Rect(tempPos, new Vector2(memberListPartInner.width, PackInspectionWindow.ListRectTemplateHeight));

            Widgets.DrawBoxSolidWithOutline(temp, otherGrey, Color.gray, 2);
            Widgets.DrawHighlightIfMouseover(temp);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(temp.MoveRect(new Vector2(CommonMargin, 0f)), (potentialMembers.Count == 0 ? "RVFFA_PackCreationWindow_AddAPawn" : "RVFFA_PackCreationWindow_AddAnotherPawn").Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonInvisible(temp))
            {
                List<FloatMenuOption> potentialPawnOptions = new List<FloatMenuOption>();
                foreach (Pawn pawn in cachedAvailablePawns)
                {
                    FloatMenuOption option = new FloatMenuOption(pawn.NameShortColored, () =>
                    {
                        potentialMembers.Add(pawn);
                        fullTempPackMembers.Add(pawn);

                        RefreshBoniInnerHeight();
                    }, pawn, Color.white);
                    potentialPawnOptions.Add(option);
                }
                Find.WindowStack.Add(new FloatMenu(potentialPawnOptions));
            }
        }

        private bool DrawOpinionBar(Pawn otherMember, IEnumerable<Pawn> otherMembers, PackDef def, Rect rect, int locationModif, out string? reason)
        {
            bool nothingIllegal = true;
            int opinion = Convert.ToInt32(packWorker.EvaluateAverageOpinionForPawn(otherMember, otherMembers));

            Texture2D barTex = PackInspectionWindow.GetBarTexForOpinion(opinion, def);
            Vector2 tempPos = new Vector2(0f, PackInspectionWindow.ListRectTemplateHeight) * locationModif + rect.position;
            Rect tempRect = new Rect(tempPos, new Vector2(rect.width, PackInspectionWindow.ListRectTemplateHeight));

            Rect tempMemberName = tempRect.LeftPartPixels(100f).MoveRect(new Vector2(CommonMargin, CommonMargin));
            Rect tempOpinionPre = tempRect.RightPartPixels(tempRect.height * 2f);
            Rect tempOpinion = tempOpinionPre.ContractedBy(CommonMargin);
            Rect tempBar = new Rect(tempMemberName.xMax + CommonMargin, tempRect.y, tempRect.width - tempMemberName.width - CommonMargin * 2 - tempOpinionPre.width, PackInspectionWindow.ListRectTemplateHeight).ContractedBy(CommonMargin);
            Rect tempCloseButton = tempRect.RightPartPixels(tempRect.height).ContractedBy(CommonMargin);

            tempRect.DoRectHighlight(locationModif % 2 == 0);
            MouseoverSounds.DoRegion(tempRect);
            Widgets.DrawHighlightIfMouseover(tempRect);
            if (Widgets.ButtonInvisible(tempRect.LeftPartPixels(200f))) Find.WindowStack.Add(new Dialog_InfoCard(otherMember));
            if (!packWorker.PawnCanJoinPack(otherMembers, otherMember, false, true, out reason))
            {
                nothingIllegal = false;
                Widgets.DrawBoxSolid(tempRect, new Color(1f, 0f, 0f, .2f));
                TooltipHandler.TipRegion(tempRect, reason);
            }

            Widgets.Label(tempMemberName, otherMember.NameShortColored);
            Widgets.Label(tempOpinion, opinion.ToString());
            Widgets.FillableBar(tempBar, (100f + opinion) / 200f, barTex);
            Widgets.DrawTextureFitted(tempCloseButton, TexButton.Delete, 1f);
            if (Widgets.ButtonInvisible(tempCloseButton))
            {
                fullTempPackMembers.Remove(otherMember);
                potentialMembers.Remove(otherMember);
                RefreshBoniInnerHeight();
                SoundDefOf.TabClose.PlayOneShotOnCamera();
            }

            return nothingIllegal;
        }

        private List<FloatMenuOption> Options
        {
            get
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (PackDef def in DefDatabase<PackDef>.AllDefsListForReading.Where(def => !def.unique))
                {
                    FloatMenuOption option = new FloatMenuOption(def.LabelCap, () =>
                    {
                        packDef = def;
                        packWorker = def.GetNewPackWorker;
                        RefreshBoniInnerHeight();
                    });

                    options.Add(option);
                }

                return options;
            }
        }

        private void RefreshBoniInnerHeight()
        {
            cachedAvailablePawns = firstPawn.Map.mapPawns.FreeColonistsSpawned.Where(pawn => !pawn.IsInPack() && pawn != firstPawn && !potentialMembers.Contains(pawn));
            boniListPartInner = boniListPartOuter.GetInnerScrollRect(PackInspectionWindow.ListRectTemplateHeight * packDef?.memberHediffs.Count ?? 0);
            memberListPartInner = memberListPartOuter.GetInnerScrollRect(PackInspectionWindow.ListRectTemplateHeight * (potentialMembers.Count + (CanAddMorePawnsToPack ? 1 : 0)));
        }

        private bool CanAddMorePawnsToPack => potentialMembers.Count < (packDef?.maxSize ?? 0) && cachedAvailablePawns.Any();

        private void DrawStatusbar()
        {
            if (packDef == null) return;
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(statusPart, "RVFFA_PackInspectionWindow_StatusBar".Translate(firstPawn.NameShortColored, packWorker.EvaluateAverageOpinionForPawn(firstPawn, potentialMembers).ToString("0.##"), packWorker.EvaluateAverageOpinionForEveryPawn(fullTempPackMembers).ToString("0.##")));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }
    }
}
