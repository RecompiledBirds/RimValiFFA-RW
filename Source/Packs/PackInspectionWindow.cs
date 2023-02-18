using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR;
using RVCRestructured.Windows;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimValiFFARW.Packs
{
    public class PackInspectionWindow : ITab
    {
        internal static float ListRectTemplateHeight = 30f;
        private const float MaxMemberListHeight = 140f;
        private const float CommonMargin = 5f;

        private static readonly Vector2 winSize = new Vector2(600, 333);
        private static readonly Rect main = new Rect(CommonMargin, 0f, winSize.x - CommonMargin * 2f, winSize.y - CommonMargin); //695 x 390
        //private readonly Color otherGrey = new Color(.15f, .15f, .15f, .10f);
        private readonly Rect titlePart;
        private readonly Rect contentPart;
        private readonly Rect packEditButton;
        private readonly Rect descriptionPart;
        private readonly Rect packRenameButton;
        private readonly Rect boniListPartOuter;

        private Vector2 newPackButtonSize = new Vector2(250f, 30f);
        private Vector2 descriptionScrollVector;
        private Vector2 memberScrollVector;
        private Vector2 boniScrollVector;
        private Rect memberListPartOuter;
        private Rect memberListPartInner;
        private Rect boniListPartInner;
        private Rect statusBar;

        private static PackInspectionWindow packInspectionWindow;

        private bool playerWasNotifiedOfPackMissing = false;
        private bool renameMode = false;
        private List<Pawn> packMembers;
        private Pack pack;
        private Pawn pawn;

        public static PackInspectionWindow GetCurrentPackInspectionWindow => packInspectionWindow;

        public Pack CurrentPack
        {
            get => pack;
            set => pack = value;
        }

        //public override bool IsVisible => SelPawn.IsInPack();

        public PackInspectionWindow()
        {
            size = winSize;
            labelKey = "RVFFA_PackInspectionWindow_LabelKey";
            packInspectionWindow = this;

            titlePart = main.TopPartPixels(30f);
            contentPart = new Rect(titlePart.x, titlePart.yMax + CommonMargin, main.width, 125f);
            packEditButton = new Rect(titlePart.xMax - (18f * 2f) - CommonMargin, titlePart.y + 4f, 18f, 18f);
            packRenameButton = packEditButton.MoveRect(new Vector2(-(packEditButton.width + CommonMargin), 0f));
            descriptionPart = new Rect(contentPart.x, contentPart.y, main.width * .7f - CommonMargin * 2f, contentPart.height);
            boniListPartOuter = new Rect(descriptionPart.xMax + CommonMargin * 2f, contentPart.y, main.width * .3f, contentPart.height).Rounded();
        }

        public override void OnOpen()
        {
            Packmanager.GetLastActivePackmanager.TryGetPackForPawn(SelPawn, out Pack pack);

            pawn = SelPawn;
            this.pack = pack;
            renameMode = false;
            base.OnOpen();

            if (pack is null) return;
            packMembers = pack.Members.ToList();

            float memberListHeight = Mathf.Min(MaxMemberListHeight, ListRectTemplateHeight * (pack.Members.Count - 1));
            memberListPartOuter = new Rect(titlePart.x, boniListPartOuter.yMax + CommonMargin * 2f, main.width, memberListHeight);
            statusBar = new Rect(titlePart.x, memberListPartOuter.yMax + CommonMargin, main.width, 20f);

            boniListPartInner = boniListPartOuter.GetInnerScrollRect(ListRectTemplateHeight * pack.Def.memberHediffs.Count);
            memberListPartInner = memberListPartOuter.GetInnerScrollRect(ListRectTemplateHeight * (pack.Members.Count - 1));

            size = winSize - new Vector2(0, MaxMemberListHeight - memberListHeight);
            UpdateSize();
        }

        protected override void FillTab()
        {
            if (pawn != SelPawn) OnOpen();

            DrawTitlePart();
            DrawPackInfo();
        }

        private void DrawPackInfo()
        {
            if (pack == null) return;
            DrawSeparators();
            DrawDescriptionPart();
            DrawBoniList(boniListPartOuter, boniListPartInner, pack.Def, ref boniScrollVector);
            DrawMemberList();
            DrawStatusbar();
        }

        private void DrawSeparators()
        {
            GUI.color = Color.gray;
            Widgets.DrawLineVertical(descriptionPart.xMax + CommonMargin, descriptionPart.y, descriptionPart.height);

            Widgets.DrawLineHorizontal(titlePart.x, titlePart.yMax, titlePart.width);
            Widgets.DrawLineHorizontal(contentPart.x, contentPart.yMax + CommonMargin, contentPart.width);
            Widgets.DrawLineHorizontal(memberListPartOuter.x, memberListPartOuter.yMax + CommonMargin, memberListPartOuter.width);
            GUI.color = Color.white;
        }

        private void DrawStatusbar()
        {
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(statusBar, "RVFFA_PackInspectionWindow_StatusBar".Translate(SelPawn.NameShortColored, pack.Worker.EvaluateAverageOpinionForPawn(SelPawn, pack.Members).ToString("0.##"), pack.Worker.EvaluateAverageOpinionForEveryPawn(pack).ToString("0.##")));

            Text.Anchor = TextAnchor.UpperCenter;
            
            Rect tempDeleteButtonLabel = new Rect(statusBar.xMax - 90f, statusBar.y, 90f, statusBar.height);
            Rect tempDeleteButton = new Rect(statusBar.xMax - 90f, statusBar.y + 2f, 90f, statusBar.height - 6f);
            Widgets.DrawBoxSolidWithOutline(tempDeleteButton, new Color(1f, 0f, 0f, .2f), Color.gray);
            Widgets.DrawHighlightIfMouseover(tempDeleteButton);
            Widgets.Label(tempDeleteButtonLabel, "RVFFA_PackInspectionWindow_DeletePack".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            
            if (Widgets.ButtonInvisible(tempDeleteButton))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new ConfirmationWindow(DisbandAction, () => { }, "RVFFA_PackInspectionWindow_DeletePackSure".Translate(pack.NameColored), "RVFFA_PackInspectionWindow_DeletePackSureTitle".Translate()));
            }
        }

        private void DisbandAction()
        {
            pack.Worker.Disband(pack);
            OnOpen();
        }

        private void DrawMemberList()
        {
            bool selfEncountered = false;

            Widgets.BeginScrollView(memberListPartOuter, ref memberScrollVector, memberListPartInner);
            for (int i = 0; i < packMembers.Count; i++)
            {
                Pawn otherMember = packMembers[i];

                selfEncountered |= otherMember == SelPawn;
                if (otherMember == SelPawn) continue;

                int multiplier = i - (selfEncountered ? 1 : 0);
                DrawOpinionBar(otherMember, pack.Def, memberListPartInner, multiplier, SelPawn.relations.OpinionOf(otherMember), out Rect _);
            }
            Widgets.EndScrollView();
        }

        private static void DrawOpinionBar(Pawn otherMember, PackDef def, Rect rect, int locationModif, int opinion, out Rect tempRect)
        {
            Texture2D barTex = GetBarTexForOpinion(opinion, def);

            Vector2 tempPos = new Vector2(0f, ListRectTemplateHeight) * locationModif + rect.position;
            tempRect = new Rect(tempPos, new Vector2(rect.width, ListRectTemplateHeight));

            Rect tempMemberName = tempRect.LeftPartPixels(100f).MoveRect(new Vector2(CommonMargin, CommonMargin));
            Rect tempOpinionPre = tempRect.RightPartPixels(tempRect.height);
            Rect tempOpinion = tempOpinionPre.ContractedBy(CommonMargin);
            Rect tempBar = new Rect(tempMemberName.xMax + CommonMargin, tempRect.y, tempRect.width - tempMemberName.width - CommonMargin * 2 - tempOpinionPre.width, ListRectTemplateHeight).ContractedBy(CommonMargin);

            tempRect.DoRectHighlight(locationModif % 2 == 0);
            MouseoverSounds.DoRegion(tempRect);
            Widgets.DrawHighlightIfMouseover(tempRect);
            if (Widgets.ButtonInvisible(tempRect)) Find.WindowStack.Add(new Dialog_InfoCard(otherMember));

            Widgets.Label(tempMemberName, otherMember.NameShortColored);
            Widgets.Label(tempOpinion, opinion.ToString());
            Widgets.FillableBar(tempBar, (100f + opinion) / 200f, barTex);
        }

        internal static void DrawBoniList(Rect boniListPartOuter, Rect boniListPartInner, PackDef def, ref Vector2 boniScrollVector)
        {
            //TODO: Background Image?
            Widgets.BeginScrollView(boniListPartOuter, ref boniScrollVector, boniListPartInner);

            for (int i = 0; i < def.memberHediffs.Count; i++)
            {
                Vector2 tempPos = new Vector2(0f, ListRectTemplateHeight) * i + boniListPartInner.position;
                Rect temp = new Rect(tempPos, new Vector2(boniListPartInner.width, ListRectTemplateHeight));

                Rect tempLabel = temp.MoveRect(new Vector2(CommonMargin, CommonMargin));
                Rect tempInfo = temp.RightPartPixels(temp.height).ContractedBy(CommonMargin);

                temp.DoRectHighlight(i % 2 == 0);
                Widgets.DrawHighlightIfMouseover(temp);
                MouseoverSounds.DoRegion(temp);

                Widgets.Label(tempLabel, def.memberHediffs[i].LabelCap);
                TooltipHandler.TipRegion(temp, def.memberHediffs[i].Description);
                Widgets.InfoCardButton(tempInfo, def.memberHediffs[i]);
            }

            Widgets.EndScrollView();
        }

        private void DrawDescriptionPart()
        {
            Widgets.LabelScrollable(descriptionPart, pack.Def.description, ref descriptionScrollVector);
        }

        private void DrawTitlePart()
        {
            if (pack is null)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(titlePart, "RVFFA_PackInspectionWindow_PawnNotInPackLabel".Translate(SelPawn.NameShortColored));
                Text.Font = GameFont.Small;

                if (Widgets.ButtonText(new Rect(size - newPackButtonSize - new Vector2(CommonMargin, CommonMargin), newPackButtonSize), "RVFFA_PackInspectionWindow_Creation".Translate(SelPawn.LabelShortCap))) 
                {
                    Find.WindowStack.Add(new PackCreationWindow(SelPawn));
                } 
                
                Widgets.LabelScrollable(descriptionPart, "RVFFA_PackInspectionWindow_PawnNotInPackDescription".Translate(), ref descriptionScrollVector);
                if (playerWasNotifiedOfPackMissing) return;

                //Messages.Message("RVFFA_PackInspectionWindow_PawnNotInPackLabel".Translate(SelPawn.NameShortColored), MessageTypeDefOf.RejectInput);
                playerWasNotifiedOfPackMissing = true;
                return;
            }

            if (renameMode)
            {
                string packName = pack.Name;
                CharacterCardUtility.DoNameInputRect(new Rect(titlePart.x, titlePart.y + CommonMargin, titlePart.width * 0.6f, titlePart.height - CommonMargin * 2f).Rounded(), ref packName, 12);
                pack.Name = packName;
            }
            else
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(titlePart, $"<b>{pack.NameColored}</b>");
                Text.Font = GameFont.Small;
            }

            TooltipHandler.TipRegion(packEditButton, "RVFFA_PackInspectionWindow_Edit".Translate());
            if(Widgets.ButtonImage(packEditButton, IconTextures.iconCustomize))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                Find.WindowStack.Add(new PackEditWindow(pack));
            }

            TooltipHandler.TipRegion(packRenameButton, "RVFFA_PackInspectionWindow_RenamePack".Translate());
            if (Widgets.ButtonImage(packRenameButton, IconTextures.iconRename))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                renameMode = !renameMode;
            }
        }

        internal static Texture2D GetBarTexForOpinion(int opinion, PackDef def)
        {
            Texture2D barTex = ColorTextures.BarFullTexBlue;

            if (opinion < def.minGroupOpinionNeededSustain)
            {
                barTex = ColorTextures.BarFullTexRed;
            }
            else if (opinion < def.minGroupOpinionNeededCreation)
            {
                barTex = ColorTextures.BarFullTexYellow;
            }
            else if (opinion < 80)
            {
                barTex = ColorTextures.BarFullTexGreen;
            }

            return barTex;
        }

        protected override void CloseTab()
        {
            packInspectionWindow = null;
            base.CloseTab();
        }
    }
}
