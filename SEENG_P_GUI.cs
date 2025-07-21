using Sandbox.Graphics.GUI;
using VRageMath;
using System;
using Sandbox;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRage.Game;
using System.IO;

namespace SEENG_Core
{
    public class MyGuiScreenSEENGCoreMenu : MyGuiScreenBase
    {
        private readonly List<WorkshopMod> _workshopMods;
        private readonly Action<WorkshopMod> _onModSelected;
        private WorkshopMod _selectedMod;
        private MyGuiControlListbox listbox;
        private MyGuiControlLabel tertiaryDescriptionLabel;
        private MyGuiControlLabel tertiaryBottomText;
        private readonly Loader _loader;
        public override string GetFriendlyName()
        {
            return nameof(MyGuiScreenSEENGCoreMenu); // welcome to hell fuckers
        }

        public MyGuiScreenSEENGCoreMenu(List<WorkshopMod> workshopMods, Action<WorkshopMod> onModSelected, Loader loader)
            : base(new Vector2(0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(1.5f, 0.8f), false, null, 0.6f, 1.0f)
        {
            _workshopMods = workshopMods;
            _onModSelected = onModSelected;
            _loader = loader;
            EnabledBackgroundFade = true;
            m_closeOnEsc = true;
            m_drawEvenWithoutFocus = true;
            CanHideOthers = true;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            CreateMainControls();
            CreateSecondaryWindow();
            CreateTertiaryWindow();
        }

        private void CreateMainControls()
        {
            AddCaption("SEENG Alpha 0.1");

            // Addon listbox
            listbox = new MyGuiControlListbox();
            var noneItem = new MyGuiControlListbox.Item(new StringBuilder("None"));
            listbox.Add(noneItem);
            foreach (WorkshopMod mod in _workshopMods)
            {
                var item = new MyGuiControlListbox.Item(new StringBuilder(mod.Name), userData: mod);
                listbox.Add(item);
                if (_selectedMod == mod)
                    listbox.SelectSingleItem(item);
            }
            listbox.MultiSelect = false;
            listbox.VisibleRowsCount = 14;
            listbox.Position = new Vector2(0f, -0.1f); // pos here 
            listbox.Size = new Vector2(0.3f, 0.4f);
            listbox.ItemsSelected += Listbox_ItemsSelected;
            Controls.Add(listbox);

            // Refit btn yes
            MyGuiControlButton closeBtn = new MyGuiControlButton(
                position: new Vector2(0f, 0.2f), // hey blind RIGHT HERE
                size: new Vector2(0.3f, 0.07f),
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                text: new StringBuilder("Refit Engine"),
                textScale: 0.9f,
                onButtonClick: OnCloseButtonClick,
                visualStyle: MyGuiControlButtonStyleEnum.Rectangular
            );
            closeBtn.CustomStyle = new MyGuiControlButton.StyleDefinition
            {
                NormalTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default.dds" } },
                HighlightTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_red_highlight.dds" } },
                FocusTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_focus.dds" } },
                ActiveTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_active.dds" } },
                NormalFont = "Green",
                HighlightFont = "Red",
                Padding = new MyGuiBorderThickness
                {
                    Left = 7f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                    Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
                }
            };
            Controls.Add(closeBtn);

            // max speed wip
            var maxSpeedLabel = new MyGuiControlLabel(
                position: new Vector2(0f, 0.25f),
                text: "Max Speed WIP use '/seeng speed' instead",
                colorMask: Color.White.ToVector4(),
                textScale: 0.8f,
                font: MyFontEnum.Blue,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(maxSpeedLabel);
            var maxSpeedSlider = new MyGuiControlSlider(
                position: new Vector2(0f, 0.28f),
                minValue: 10f,
                maxValue: 100f,
                width: 0.3f,
                defaultValue: 50f,
                labelText: "sex",
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(maxSpeedSlider);

            // volume wip
            var volumeLabel = new MyGuiControlLabel(
                position: new Vector2(0f, 0.34f),
                text: "Engine Volume WIP",
                colorMask: Color.White.ToVector4(),
                textScale: 0.8f,
                font: MyFontEnum.Blue,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(volumeLabel);
            var volumeSlider = new MyGuiControlSlider(
                position: new Vector2(0f, 0.37f),
                minValue: 0f,
                maxValue: 100f,
                width: 0.3f,
                defaultValue: 50f,
                labelText: "",
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(volumeSlider);
        }

        private void CreateSecondaryWindow()
        {
            // SECOND RIGHT WINDOW
            var secondaryBackground = new MyGuiControlCompositePanel()
            {
                Position = new Vector2(0.35f, 0f),
                Size = new Vector2(0.4f, 0.7f),
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK,
                ColorMask = MyGuiConstants.SCREEN_BACKGROUND_COLOR
            };
            Controls.Add(secondaryBackground);

            // Add ifo
            var secondaryLabel = new MyGuiControlLabel(
                position: new Vector2(0.35f, -0.3f),
                text: "Additional Info",
                colorMask: Color.White.ToVector4(),
                textScale: 2.0f,
                font: MyFontEnum.Blue,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(secondaryLabel);

            // More info
            var infoText = new MyGuiControlLabel(
                position: new Vector2(0.35f, -0.15f),
                text: "This mod is in very active development\nexpect many bugs and crashes\nengines may sound incorect due to\nsome features are" +
                " not finished yet\n\nreport any problems n suggestions\nto ma beautiful discord (scary place)\n" +
                "\nthe max speed defines at what\nspeed engine hits its peak sound",
                colorMask: Color.LightGray.ToVector4(),
                textScale: 0.9f,
                font: MyFontEnum.White,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(infoText);

            // hi
            var icon = new MyGuiControlImage(
                position: new Vector2(0.35f, 0.1f),
                size: new Vector2(0.15f, 0.15f),
                backgroundTexture: null
            );
            Controls.Add(icon);

            // btns 3
            float buttonHeight = 0.07f;
            float totalButtonSpace = buttonHeight * 3 + 0.02f;
            float startY = 0.25f - (totalButtonSpace / 2);

            MyGuiControlButton button1 = new MyGuiControlButton(
                position: new Vector2(0.35f, startY),
                size: new Vector2(0.3f, buttonHeight),
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                text: new StringBuilder("Get more engines!"),
                textScale: 0.9f,
                onButtonClick: (b) => MyGuiSandbox.OpenUrlWithFallback("https://steamcommunity.com/sharedfiles/filedetails/?id=3495022512", "kss"),
                visualStyle: MyGuiControlButtonStyleEnum.Rectangular
            );
            button1.CustomStyle = new MyGuiControlButton.StyleDefinition
            {
                NormalTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default.dds" } },
                HighlightTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_highlight.dds" } },
                FocusTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_focus.dds" } },
                ActiveTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_active.dds" } },
                NormalFont = "Blue",
                HighlightFont = "Blue",
                Padding = new MyGuiBorderThickness
                {
                    Left = 7f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                    Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
                }
            };
            Controls.Add(button1);

            MyGuiControlButton button2 = new MyGuiControlButton(
                position: new Vector2(0.35f, startY + buttonHeight + 0.01f),
                size: new Vector2(0.3f, buttonHeight),
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                text: new StringBuilder("Discorda"),
                textScale: 0.9f,
                onButtonClick: (b) => MyGuiSandbox.OpenUrlWithFallback("https://discord.gg/bvkhT6wvDm", "kks"),
                visualStyle: MyGuiControlButtonStyleEnum.Rectangular
            );
            button2.CustomStyle = new MyGuiControlButton.StyleDefinition
            {
                NormalTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default.dds" } },
                HighlightTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_highlight.dds" } },
                FocusTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_focus.dds" } },
                ActiveTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_active.dds" } },
                NormalFont = "Blue",
                HighlightFont = "Blue",
                Padding = new MyGuiBorderThickness
                {
                    Left = 7f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                    Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
                }
            };
            Controls.Add(button2);

            MyGuiControlButton button3 = new MyGuiControlButton(
                position: new Vector2(0.35f, startY + (buttonHeight * 2) + 0.02f),
                size: new Vector2(0.3f, buttonHeight),
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER,
                text: new StringBuilder("HowTo your own engine(soon)"),
                textScale: 0.9f,
                onButtonClick: (b) => MyGuiSandbox.OpenUrlWithFallback("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "kks"),
                visualStyle: MyGuiControlButtonStyleEnum.Rectangular
            );
            button3.CustomStyle = new MyGuiControlButton.StyleDefinition
            {
                NormalTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_red.dds" } },
                HighlightTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_red_highlight.dds" } },
                FocusTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_focus.dds" } },
                ActiveTexture = new MyGuiCompositeTexture { Center = new MyGuiSizedTexture { Texture = "Textures\\GUI\\Controls\\button_skins_default_active.dds" } },
                NormalFont = "Blue",
                HighlightFont = "Red",
                Padding = new MyGuiBorderThickness
                {
                    Left = 7f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Right = 5f / MyGuiConstants.GUI_OPTIMAL_SIZE.X,
                    Top = 6f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y,
                    Bottom = 10f / MyGuiConstants.GUI_OPTIMAL_SIZE.Y
                }
            };
            Controls.Add(button3);
        }

        private void CreateTertiaryWindow()
        {
            // 3 LEFT WNDW
            var tertiaryBackground = new MyGuiControlCompositePanel()
            {
                Position = new Vector2(-0.35f, 0f), 
                Size = new Vector2(0.4f, 0.7f),
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK,
                ColorMask = MyGuiConstants.SCREEN_BACKGROUND_COLOR
            };
            Controls.Add(tertiaryBackground);

            // Ifnf
            var tertiaryTopLabel = new MyGuiControlLabel(
                position: new Vector2(-0.35f, -0.3f),
                text: "Engine specifications",
                colorMask: Color.White.ToVector4(),
                textScale: 1.8f,
                font: MyFontEnum.Blue,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER
            );
            Controls.Add(tertiaryTopLabel);

            // Addon main info
            tertiaryDescriptionLabel = new MyGuiControlLabel(
                position: new Vector2(-0.50f, -0.26f),
                text: "Select a pack...",
                colorMask: Color.LightGray.ToVector4(),
                textScale: 0.8f,
                font: MyFontEnum.White,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            );
            Controls.Add(tertiaryDescriptionLabel);

            // For Ship picture 
            var innerWindow = new MyGuiControlCompositePanel()
            {
                Position = new Vector2(-0.35f, 0f), 
                Size = new Vector2(0.4f * 0.33f * 2.5f, 0.7f * 0.33f), // 2.5 
                BackgroundTexture = MyGuiConstants.TEXTURE_RECTANGLE_DARK,
                ColorMask = new Vector4(0.5f, 0.5f, 0.5f, 0.8f) // grey
            };
            Controls.Add(innerWindow); // engine picture wshould be here but yeah... ебать рот

            // big engine descrp
            tertiaryBottomText = new MyGuiControlLabel(
                position: new Vector2(-0.50f, 0.13f), 
                text: "Select a pack for details...",
                colorMask: Color.LightGray.ToVector4(),
                textScale: 1.0f,
                font: MyFontEnum.White,
                originAlign: MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
            );
            Controls.Add(tertiaryBottomText);
        }

        private void Listbox_ItemsSelected(MyGuiControlListbox listbox)
        {
            if (listbox.SelectedItems.Count > 0)
            {
                _selectedMod = listbox.SelectedItems[0].UserData as WorkshopMod;
                // 
                string descPath = _selectedMod != null ? Path.Combine(_selectedMod.ModPath, "Data", "seengm_enginename_desc.txt") : "";
                string description = string.IsNullOrEmpty(descPath) || !File.Exists(descPath) ? "No description available." : File.ReadAllText(descPath).Trim();
                tertiaryDescriptionLabel.Text = description;

                // seengm_engineBigDesc.txt
                string bigDescPath = _selectedMod != null ? Path.Combine(_selectedMod.ModPath, "Data", "seengm_engineBigDesc.txt") : "";
                string bigDescription = string.IsNullOrEmpty(bigDescPath) || !File.Exists(bigDescPath) ? "No detailed description available." : File.ReadAllText(bigDescPath).Trim();
                tertiaryBottomText.Text = bigDescription;

                //  opn
                if (_selectedMod != null)
                {
                    TextureOverlayRenderer.StartOverlay(_selectedMod.ModPath);
                }
            }
            else
            {
                _selectedMod = null;
                tertiaryDescriptionLabel.Text = "Select a pack for description...";
                tertiaryBottomText.Text = "Select a pack for details...";
                TextureOverlayRenderer.CloseOverlay();
            }
        }

        private void OnCloseButtonClick(MyGuiControlButton closeBtn)
        {
            _onModSelected?.Invoke(_selectedMod);
            TextureOverlayRenderer.CloseOverlay(); // close refit btn
            CloseScreen();
        }
        public string GetModPath()
        {
            return _selectedMod?.ModPath ?? @"";
        }
    }
}