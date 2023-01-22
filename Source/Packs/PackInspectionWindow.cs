using RimWorld;
using RVCRestructured;
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
        private const float listRectTemplateHeight = 30f;
        private const float CommonMargin = 5f;

        private static readonly Vector2 winSize = new Vector2(700, 400);
        private static readonly Rect main = new Rect(CommonMargin, 0f, winSize.x - CommonMargin * 2f, winSize.y - CommonMargin); //695 x 390
        private readonly Color otherGrey = new Color(.15f, .15f, .15f, .10f);
        private readonly Rect titlePart;
        private readonly Rect descriptionPart;
        private readonly Rect boniListPartOuter;
        private readonly Rect memberListPartOuter;
        private readonly Rect statusBar;

        private bool playerWasNotifiedOfPackMissing = false;
        private Vector2 descriptionScrollVector;
        private Vector2 memberScrollVector;
        private Vector2 boniScrollVector;
        private Rect memberListPartInner;
        private Rect boniListPartInner;

        private static PackInspectionWindow packInspectionWindow;
        private List<Pawn> packMembers;
        private Pack pack;

        public static PackInspectionWindow GetCurrentPackInspectionWindow => packInspectionWindow;

        public Pack CurrentPack
        {
            get => pack;
            set => pack = value;
        }

        public override bool IsVisible => base.IsVisible;

        public PackInspectionWindow()
        {
            size = winSize;
            labelKey = "RVFFA_PackInspectionWindow_LabelKey";
            packInspectionWindow = this;

            titlePart = main.TopPartPixels(30f);
            descriptionPart = new Rect(titlePart.x, titlePart.yMax + CommonMargin, main.width, 75f);
            boniListPartOuter = new Rect(descriptionPart.x, descriptionPart.yMax + CommonMargin * 2f, main.width, 105f);
            memberListPartOuter = new Rect(boniListPartOuter.x, boniListPartOuter.yMax + CommonMargin, main.width, 140f);
            statusBar = new Rect(memberListPartOuter.x, memberListPartOuter.yMax + CommonMargin, main.width, 20f);
        }

        public override void OnOpen()
        {
            Packmanager.GetLastActivePackmanager.TryGetPackForPawn(SelPawn, out Pack pack);
            this.pack = pack;
            base.OnOpen();

            if (pack is null) return;
            packMembers = pack.Members.ToList();

            boniListPartInner = boniListPartOuter.GetInnerScrollRect(listRectTemplateHeight * pack.Def.memberHediffs.Count);
            memberListPartInner = memberListPartOuter.GetInnerScrollRect(listRectTemplateHeight * pack.Members.Count);
        }

        protected override void FillTab()
        {
            //Widgets.DrawBox(TitlePart);
            //Widgets.DrawBox(descriptionPart);

            Widgets.DrawBoxSolidWithOutline(boniListPartOuter, otherGrey, Color.gray);
            Widgets.DrawBoxSolidWithOutline(memberListPartOuter, otherGrey, Color.gray);

            DrawTitlePart();
            DrawDescriptionPart();
            DrawBoniList();
            DrawMemberList();
            DrawStatusbar();

            //TODO: Relation Average
            //TODO: Pack Type and related Boni
        }

        private void DrawStatusbar()
        {
            if (pack == null) return;
            GUI.color = Color.gray;
            Text.Font = GameFont.Tiny;
            Widgets.Label(statusBar, "RVFFA_PackInspectionWindow_StatusBar".Translate(SelPawn.NameShortColored, pack.Worker.EvaluateAverageOpinionForMember(SelPawn, pack), pack.Worker.EvaluateAverageOpinionForEveryMember(pack)));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private void DrawMemberList()
        {
            if (pack is null) return;
            Widgets.BeginScrollView(memberListPartOuter, ref memberScrollVector, memberListPartInner);
            for (int i = 0; i < packMembers.Count; i++)
            {
                Pawn otherMember = packMembers[i];
                if (otherMember == SelPawn) continue;
                int opinion = SelPawn.relations.OpinionOf(otherMember);
                Texture2D barTex = GetBarTexForOpinion(opinion);

                Vector2 tempPos = new Vector2(0f, listRectTemplateHeight) * i + memberListPartInner.position;
                Rect temp = new Rect(tempPos, new Vector2(memberListPartInner.width, listRectTemplateHeight));

                Rect tempMemberName = temp.LeftPartPixels(100f).MoveRect(new Vector2(CommonMargin, CommonMargin));
                Rect tempOpinionPre = temp.RightPartPixels(temp.height);
                Rect tempOpinion = tempOpinionPre.ContractedBy(CommonMargin);
                Rect tempBar = new Rect(tempMemberName.xMax + CommonMargin, temp.y, temp.width - tempMemberName.width - CommonMargin * 2 - tempOpinionPre.width, listRectTemplateHeight).ContractedBy(CommonMargin);

                temp.DoRectHighlight(i % 2 == 0);
                MouseoverSounds.DoRegion(temp);
                Widgets.DrawHighlightIfMouseover(temp);
                if (Widgets.ButtonInvisible(temp)) Find.WindowStack.Add(new Dialog_InfoCard(otherMember));

                Widgets.Label(tempMemberName, otherMember.NameShortColored);
                Widgets.Label(tempOpinion, opinion.ToString());
                Widgets.FillableBar(tempBar, (100f + opinion) / 200f, barTex);
            }
            Widgets.EndScrollView();
        }

        private void DrawBoniList()
        {
            //TODO: Background Image?
            if (pack is null) return;

            Widgets.BeginScrollView(boniListPartOuter, ref boniScrollVector, boniListPartInner);

            for (int i = 0; i < pack.Def.memberHediffs.Count; i++)
            {
                Vector2 tempPos = new Vector2(0f, listRectTemplateHeight) * i + boniListPartInner.position;
                Rect temp = new Rect(tempPos, new Vector2(boniListPartInner.width, listRectTemplateHeight));

                Rect tempLabel = temp.LeftPartPixels(100f).MoveRect(new Vector2(CommonMargin, CommonMargin));
                Rect tempInfo = temp.RightPartPixels(temp.height).ContractedBy(CommonMargin);
                Rect tempDesc = new Rect(tempLabel.xMax + CommonMargin, temp.y, temp.width - tempLabel.width - CommonMargin * 2 - tempInfo.width, listRectTemplateHeight).MoveRect(new Vector2(CommonMargin, CommonMargin));

                temp.DoRectHighlight(i % 2 == 0);
                Widgets.DrawHighlightIfMouseover(temp);
                MouseoverSounds.DoRegion(temp);

                Widgets.Label(tempLabel, pack.Def.memberHediffs[i].LabelCap);
                Widgets.Label(tempDesc, pack.Def.memberHediffs[i].Description);
                Widgets.InfoCardButton(tempInfo, pack.Def.memberHediffs[i]);
            }

            Widgets.EndScrollView();
        }

        private void DrawDescriptionPart()
        {
            Widgets.LabelScrollable(descriptionPart, pack?.Def.description ?? "RVFFA_PackInspectionWindow_PawnNotInPackDescription".Translate(), ref descriptionScrollVector);

            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(descriptionPart.x, descriptionPart.yMax + CommonMargin, descriptionPart.width);
            GUI.color = Color.white;
        }

        private void DrawTitlePart()
        {
            if (pack is null)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(titlePart, "RVFFA_PackInspectionWindow_PawnNotInPackLabel".Translate(SelPawn.NameShortColored));
                Text.Font = GameFont.Small;

                if (playerWasNotifiedOfPackMissing) return;

                Messages.Message("RVFFA_PackInspectionWindow_PawnNotInPackLabel".Translate(SelPawn.NameShortColored), MessageTypeDefOf.RejectInput);
                playerWasNotifiedOfPackMissing = true;
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(titlePart, pack.NameColored);
            Text.Font = GameFont.Small;

            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(titlePart.x, titlePart.yMax, titlePart.width);
            GUI.color = Color.white;
        }

        private Texture2D GetBarTexForOpinion(int opinion)
        {
            Texture2D barTex = ColorTextures.BarFullTexBlue;

            if (opinion < pack.Def.minGroupOpinionNeededSustain)
            {
                barTex = ColorTextures.BarFullTexRed;
            }
            else if (opinion < pack.Def.minGroupOpinionNeededCreation)
            {
                barTex = ColorTextures.BarFullTexYellow;
            }
            else if (opinion < 80)
            {
                barTex = ColorTextures.BarFullTexGreen;
            }

            return barTex;
        }
    }
}
