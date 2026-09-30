using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    internal static class EnvironmentStagingTextureBuilder
    {
        internal const string EnvironmentAtlasPath = "Assets/Art/Effects/World/VFX_EnvironmentPixelStaging.asset";
        internal const string RainAtlasPath = "Assets/Art/Effects/World/VFX_RainPixelStaging.asset";

        private const int SceneWidth = 480;
        private const int SceneHeight = 270;
        private const int AtlasWidth = SceneWidth * 2;

        internal static void EnsureAssets()
        {
            EnsureFolder("Assets/Art/Effects/World");
            EnsureEnvironmentAtlas();
            EnsureRainAtlas();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(EnvironmentAtlasPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(RainAtlasPath, ImportAssetOptions.ForceSynchronousImport);
        }

        internal static Sprite GetEnvironmentSprite(bool mert)
        {
            string expectedName = mert ? "VFX_MertApartment_PixelStaging" : "VFX_ErenApartment_PixelStaging";
            return AssetDatabase.LoadAllAssetsAtPath(EnvironmentAtlasPath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == expectedName);
        }

        internal static Sprite GetRainSprite(int index)
        {
            string expectedName = $"VFX_RainStreak_{Mathf.Abs(index) % 8:00}";
            return AssetDatabase.LoadAllAssetsAtPath(RainAtlasPath)
                .OfType<Sprite>()
                .FirstOrDefault(sprite => sprite.name == expectedName);
        }

        private static void EnsureEnvironmentAtlas()
        {
            Texture2D texture = GetOrCreateTexture(EnvironmentAtlasPath, AtlasWidth, SceneHeight,
                "VFX_EnvironmentPixelStaging_Texture");
            var canvas = new PixelCanvas(AtlasWidth, SceneHeight);
            DrawEren(canvas, 0);
            DrawMert(canvas, SceneWidth);
            texture.SetPixels32(canvas.Pixels);
            texture.Apply(false, false);
            EditorUtility.SetDirty(texture);

            EnsureSprite(texture, EnvironmentAtlasPath, "VFX_ErenApartment_PixelStaging",
                new Rect(0f, 0f, SceneWidth, SceneHeight), 16f);
            EnsureSprite(texture, EnvironmentAtlasPath, "VFX_MertApartment_PixelStaging",
                new Rect(SceneWidth, 0f, SceneWidth, SceneHeight), 16f);
        }

        private static void EnsureRainAtlas()
        {
            const int cellWidth = 8;
            const int cellHeight = 16;
            const int columns = 8;
            Texture2D texture = GetOrCreateTexture(RainAtlasPath, cellWidth * columns, cellHeight,
                "VFX_RainPixelStaging_Texture");
            var canvas = new PixelCanvas(cellWidth * columns, cellHeight);
            canvas.Clear(new Color32(0, 0, 0, 0));

            Color32 dim = C("7894a7", 120);
            Color32 pale = C("b6d5dd", 185);
            for (int frame = 0; frame < columns; frame++)
            {
                int ox = frame * cellWidth;
                int start = 1 + frame % 4;
                int length = 7 + (frame * 3 % 8);
                for (int step = 0; step < length; step++)
                {
                    int y = start + step;
                    if (y >= cellHeight)
                    {
                        break;
                    }

                    int x = ox + 5 - (step + frame) / 5;
                    canvas.Pixel(x, y, step == length - 1 || step == 0 ? dim : pale);
                    if (frame % 3 == 0 && step > 2 && step < length - 2 && step % 3 == 0)
                    {
                        canvas.Pixel(x + 1, y, dim);
                    }
                }
            }

            texture.SetPixels32(canvas.Pixels);
            texture.Apply(false, false);
            EditorUtility.SetDirty(texture);
            for (int frame = 0; frame < columns; frame++)
            {
                EnsureSprite(texture, RainAtlasPath, $"VFX_RainStreak_{frame:00}",
                    new Rect(frame * cellWidth, 0f, cellWidth, cellHeight), 16f);
            }
        }

        private static void DrawEren(PixelCanvas c, int ox)
        {
            Color32 night = C("070d18");
            Color32 skyA = C("081322");
            Color32 skyB = C("0e2134");
            Color32 far = C("102536");
            Color32 mid = C("153247");
            Color32 near = C("081522");
            Color32 frame = C("111722");
            Color32 frameEdge = C("2a3b4a");
            Color32 wall = C("211d23");
            Color32 wallWarm = C("3b2827");
            Color32 floor = C("131922");
            Color32 floorEdge = C("252e38");
            Color32 wood = C("523126");
            Color32 woodEdge = C("8c5840");
            Color32 cloth = C("4b3036");
            Color32 clothLight = C("8a5960");
            Color32 cyan = C("54bfca");
            Color32 cyanDim = C("285a69");
            Color32 amber = C("d49343");
            Color32 paper = C("b3a989");
            Color32 ink = C("080d14");

            c.Rect(ox, 0, SceneWidth, SceneHeight, night);
            c.Rect(ox, 17, SceneWidth, 46, skyA);
            c.Rect(ox, 63, SceneWidth, 96, skyB);
            c.Dither(ox, 20, SceneWidth, 135, C("203d51"), 71, 0);

            // Far skyline: low contrast, stepped roofs and sparse inhabited windows.
            for (int i = 0; i < 18; i++)
            {
                int x = ox - 9 + i * 29;
                int width = 21 + (i * 7 % 19);
                int top = 52 + (i * 19 % 48);
                c.SteppedBuilding(x, top, width, 160 - top, far, C("1c3447"), i);
                c.WindowCluster(x + 5, top + 14, width - 10, 160 - top - 18,
                    C("405f6c"), C("81623b"), i * 13, 10);
            }

            // Mid band uses narrower masses, service caps, bridges, and broken window groupings.
            for (int i = 0; i < 12; i++)
            {
                int x = ox + 8 + i * 42;
                int width = 30 + (i * 5 % 14);
                int top = 66 + (i * 23 % 57);
                c.SteppedBuilding(x, top, width, 166 - top, mid, C("285066"), i + 31);
                c.Rect(x + 4, top - 4, Math.Max(8, width - 11), 4, C("213c4f"));
                if (i % 3 == 0)
                {
                    c.Rect(x + width - 8, top - 13, 4, 13, C("172c3d"));
                    c.Line(x + width - 6, top - 13, x + width - 4, top - 25, C("314e5e"));
                }
                c.WindowCluster(x + 6, top + 15, width - 12, 156 - top,
                    C("37818c"), C("b1793c"), 71 + i * 17, 8);
            }
            c.Rect(ox + 65, 104, 126, 4, C("1d3c50"));
            c.Rect(ox + 72, 108, 113, 3, C("0e2030"));
            c.Rect(ox + 326, 87, 96, 3, C("294b5d"));

            // Near silhouettes frame the window without becoming flat bars.
            c.Polygon(new[]
            {
                P(ox, 71), P(ox + 31, 71), P(ox + 31, 54), P(ox + 55, 54),
                P(ox + 55, 160), P(ox, 160)
            }, near);
            c.Rect(ox + 13, 91, 6, 69, C("101f2d"));
            c.Rect(ox + 28, 81, 4, 79, C("1e3443"));
            c.Polygon(new[]
            {
                P(ox + 435, 63), P(ox + 456, 63), P(ox + 456, 45), P(ox + 480, 45),
                P(ox + 480, 160), P(ox + 435, 160)
            }, C("09131f"));
            c.Rect(ox + 449, 75, 4, 85, C("1c3040"));

            // Built-in window: thick trim, stepped corners, sill, shadow, and glass reflection fragments.
            c.Rect(ox, 0, SceneWidth, 18, frame);
            c.Rect(ox, 18, 18, 154, frame);
            c.Rect(ox + 462, 18, 18, 154, frame);
            c.Rect(ox + 14, 154, 452, 15, frame);
            c.Rect(ox + 18, 18, 444, 4, frameEdge);
            c.Rect(ox + 18, 22, 4, 132, C("354451"));
            c.Rect(ox + 458, 22, 4, 132, ink);
            foreach (int mx in new[] { 105, 205, 306, 405 })
            {
                c.Rect(ox + mx, 22, 7, 132, frame);
                c.Rect(ox + mx + 1, 22, 2, 132, C("263643"));
            }
            c.Rect(ox + 15, 154, 451, 5, C("3c4d59"));
            c.Rect(ox + 20, 159, 441, 8, C("121a24"));
            c.Dither(ox + 23, 25, 435, 127, C("4c8291", 42), 47, 1);
            c.Line(ox + 244, 37, ox + 238, 78, C("4a6f79", 42));
            c.Line(ox + 384, 87, ox + 379, 126, C("365f6b", 34));

            // Interior wall islands break up the glass and define domestic zones.
            c.Polygon(new[]
            {
                P(ox + 18, 82), P(ox + 74, 82), P(ox + 74, 154), P(ox + 18, 154)
            }, wallWarm);
            c.Rect(ox + 20, 84, 52, 3, C("5b3a39"));
            c.Polygon(new[]
            {
                P(ox + 194, 69), P(ox + 269, 69), P(ox + 269, 154), P(ox + 194, 154)
            }, wall);
            c.Rect(ox + 198, 74, 67, 3, C("3f3940"));
            c.Rect(ox + 273, 109, 34, 45, C("1a1f29"));
            c.Rect(ox + 428, 96, 34, 58, C("20242d"));

            // Floor is a material plane, never a translucent gameplay strip.
            c.Rect(ox, 169, SceneWidth, 101, floor);
            c.Rect(ox, 169, SceneWidth, 5, floorEdge);
            c.Rect(ox, 174, SceneWidth, 4, C("0e141c"));
            for (int y = 215; y < 270; y += 34)
            {
                c.Line(ox, y, ox + SceneWidth - 1, y, C("151c24"));
            }
            for (int x = 70; x < SceneWidth; x += 120)
            {
                c.Line(ox + 240, 178, ox + x, 269, C("161e26"));
            }
            c.Dither(ox, 183, SceneWidth, 87, C("27323b"), 61, 3);

            // Shelf and asymmetrical personal storage.
            c.Shadow(ox + 20, 116, 48, 58);
            c.Frame(ox + 18, 72, 54, 94, 5, ink, wood);
            for (int y = 95; y <= 145; y += 25)
            {
                c.Rect(ox + 22, y, 46, 5, woodEdge);
                c.Rect(ox + 22, y + 5, 46, 2, C("1e1718"));
            }
            Color32[] bookColors = { C("b15d3f"), cyan, C("736d61"), C("8d704d"), C("354b58") };
            for (int i = 0; i < 14; i++)
            {
                int shelfY = 92 + (i / 5) * 25;
                int bx = ox + 24 + (i % 5) * 8 + (i / 5) * 2;
                int bh = 11 + (i * 7 % 8);
                c.Rect(bx, shelfY - bh, 5 + i % 2, bh, bookColors[i % bookColors.Length]);
                c.Pixel(bx + 1, shelfY - bh + 2, C("d0b675"));
            }
            c.Rect(ox + 47, 149, 17, 10, C("3c3230"));
            c.Rect(ox + 50, 147, 11, 2, C("725044"));

            // Bed: stepped headboard, shadow, layered fabric and silhouette breaks.
            c.Shadow(ox + 72, 182, 133, 23);
            c.Polygon(new[]
            {
                P(ox + 66, 112), P(ox + 78, 112), P(ox + 78, 151), P(ox + 184, 151),
                P(ox + 190, 157), P(ox + 190, 184), P(ox + 69, 184)
            }, wood);
            c.Rect(ox + 71, 116, 5, 65, woodEdge);
            c.Rect(ox + 78, 151, 108, 7, C("9c8f75"));
            c.Polygon(new[]
            {
                P(ox + 84, 144), P(ox + 119, 142), P(ox + 129, 151), P(ox + 84, 151)
            }, C("c2b798"));
            c.Polygon(new[]
            {
                P(ox + 122, 151), P(ox + 185, 151), P(ox + 190, 159), P(ox + 181, 177),
                P(ox + 134, 175), P(ox + 126, 164)
            }, clothLight);
            c.Polygon(new[]
            {
                P(ox + 122, 151), P(ox + 142, 153), P(ox + 151, 175), P(ox + 134, 175),
                P(ox + 126, 164)
            }, C("b66b50"));
            c.Line(ox + 155, 154, ox + 169, 173, C("3e2931"));
            c.Line(ox + 131, 159, ox + 143, 173, C("6d4045"));
            c.Pixel(ox + 181, 164, C("c08362"));
            c.Rect(ox + 76, 184, 9, 9, ink);
            c.Rect(ox + 177, 181, 8, 12, ink);

            // Bedside table and practical lamp form the warm pool.
            c.Shadow(ox + 188, 185, 35, 17);
            c.Frame(ox + 190, 148, 31, 37, 4, ink, C("4c3530"));
            c.Rect(ox + 194, 158, 23, 3, C("7d5544"));
            c.Rect(ox + 194, 170, 23, 3, C("7d5544"));
            c.Rect(ox + 203, 126, 4, 23, C("835136"));
            c.Polygon(new[]
            {
                P(ox + 193, 126), P(ox + 216, 126), P(ox + 211, 113), P(ox + 198, 113)
            }, amber);
            c.Rect(ox + 196, 126, 17, 3, C("f1bd63"));
            c.Dither(ox + 158, 104, 91, 77, C("b86d3c", 82), 13, 0);
            c.Dither(ox + 183, 111, 46, 54, C("e0a34e", 95), 7, 2);
            c.Polygon(new[]
            {
                P(ox + 164, 178), P(ox + 232, 178), P(ox + 224, 189), P(ox + 174, 189)
            }, C("4f3027", 115));

            // Center circulation/door is intentionally calmer than either focal zone.
            c.Shadow(ox + 229, 164, 65, 28);
            c.Frame(ox + 221, 69, 62, 100, 6, ink, C("292429"));
            c.Frame(ox + 230, 80, 44, 77, 3, C("473737"), C("201f26"));
            c.Polygon(new[]
            {
                P(ox + 235, 87), P(ox + 268, 87), P(ox + 263, 118), P(ox + 241, 145),
                P(ox + 235, 145)
            }, C("30282d"));
            c.Rect(ox + 236, 148, 16, 3, C("735044"));
            c.Rect(ox + 263, 117, 6, 4, C("b6965d"));
            c.Rect(ox + 246, 62, 18, 4, amber);
            c.Dither(ox + 225, 65, 55, 103, C("8a573b", 52), 19, 1);

            // Investigation workstation: layered chassis, framed displays, input deck and clutter.
            c.Shadow(ox + 298, 186, 150, 25);
            c.Polygon(new[]
            {
                P(ox + 304, 92), P(ox + 407, 92), P(ox + 414, 102), P(ox + 409, 155),
                P(ox + 301, 155)
            }, C("101820"));
            c.Polygon(new[]
            {
                P(ox + 296, 151), P(ox + 446, 151), P(ox + 441, 160), P(ox + 306, 160)
            }, C("8a5038"));
            c.Rect(ox + 302, 160, 139, 8, C("1a1519"));
            c.Polygon(new[]
            {
                P(ox + 310, 168), P(ox + 326, 168), P(ox + 320, 199), P(ox + 307, 199)
            }, ink);
            c.Frame(ox + 404, 166, 32, 39, 4, ink, C("3f3030"));
            c.Rect(ox + 410, 176, 20, 3, C("715044"));
            c.Rect(ox + 410, 187, 20, 3, C("715044"));
            c.Frame(ox + 320, 101, 74, 48, 5, ink, C("1c3d49"));
            c.Rect(ox + 327, 108, 60, 32, C("31808e"));
            c.Rect(ox + 330, 111, 54, 4, C("88e0e3"));
            for (int row = 0; row < 4; row++)
            {
                c.Rect(ox + 331, 119 + row * 5, 19 + (row * 11 % 32), 2,
                    row == 2 ? C("c79046") : C("61a9b4"));
            }
            c.Rect(ox + 354, 149, 8, 6, C("293a42"));
            c.Rect(ox + 343, 155, 31, 4, ink);
            c.Polygon(new[]
            {
                P(ox + 321, 149), P(ox + 368, 149), P(ox + 373, 154), P(ox + 316, 154)
            }, C("4c5a5d"));
            for (int key = 0; key < 9; key++)
            {
                c.Rect(ox + 322 + key * 5, 151, 3, 1, C("8ba2a0"));
            }
            c.Polygon(new[]
            {
                P(ox + 379, 146), P(ox + 402, 148), P(ox + 398, 156), P(ox + 376, 154)
            }, paper);
            c.Line(ox + 381, 150, ox + 395, 152, C("5a635d"));
            c.Rect(ox + 414, 142, 9, 13, C("8a5840"));
            c.Rect(ox + 416, 140, 5, 2, C("d09b5b"));
            c.Line(ox + 386, 168, ox + 369, 195, ink);
            c.Line(ox + 369, 195, ox + 347, 191, ink);
            c.Dither(ox + 306, 96, 101, 76, C("35a3b6", 72), 17, 2);

            // A small warm practical keeps the workstation cinematic without weakening the cyan display focus.
            c.Line(ox + 405, 149, ox + 416, 130, C("6f432d"), 2);
            c.Polygon(new[]
            {
                P(ox + 410, 129), P(ox + 428, 129), P(ox + 424, 136), P(ox + 414, 136)
            }, C("d79643"));
            c.Rect(ox + 413, 136, 12, 2, C("f0bb64"));
            c.Dither(ox + 390, 120, 54, 53, C("b86d3c", 65), 17, 4);

            // Chair and under-desk silhouette create foreground depth without blocking the lane.
            c.Polygon(new[]
            {
                P(ox + 282, 155), P(ox + 300, 155), P(ox + 304, 184), P(ox + 298, 189),
                P(ox + 285, 187), P(ox + 281, 173)
            }, C("121820"));
            c.Line(ox + 288, 187, ox + 279, 207, C("0a1017"), 3);
            c.Line(ox + 299, 187, ox + 310, 207, C("0a1017"), 3);

            // Small routine fragments and cable runs.
            c.Polygon(new[]
            {
                P(ox + 169, 146), P(ox + 181, 146), P(ox + 183, 151), P(ox + 167, 151)
            }, paper);
            c.Rect(ox + 184, 140, 6, 11, C("6c5947"));
            c.Rect(ox + 185, 138, 4, 2, C("5fa4aa"));
            c.Line(ox + 334, 168, ox + 329, 194, ink, 2);
            c.Line(ox + 329, 194, ox + 317, 200, ink, 2);

            // Foreground fragments are partial, stepped and deliberately sparse.
            c.Polygon(new[]
            {
                P(ox, 0), P(ox + 9, 0), P(ox + 9, 86), P(ox + 15, 86), P(ox + 15, 270), P(ox, 270)
            }, C("040810"));
            c.Polygon(new[]
            {
                P(ox + 470, 0), P(ox + 480, 0), P(ox + 480, 270), P(ox + 464, 270),
                P(ox + 464, 202), P(ox + 470, 202)
            }, C("040810"));
            c.Line(ox + 6, 31, ox + 43, 31, C("172330"), 4);
            c.Line(ox + 458, 25, ox + 451, 86, C("1a2b37"), 3);

            // Baked rain is sparse; moving sprites add only a few brighter accents.
            for (int i = 0; i < 14; i++)
            {
                int x = ox + 24 + (i * 83 % 426);
                int y = 25 + (i * 47 % 126);
                if ((x / 31 + y / 17) % 4 == 0)
                {
                    continue;
                }
                c.Line(x, y, x - 1, y + 3 + i % 5, C("718c9a", 92));
            }
        }

        private static void DrawMert(PixelCanvas c, int ox)
        {
            Color32 night = C("060c15");
            Color32 exterior = C("091a28");
            Color32 exteriorMid = C("102c3d");
            Color32 ink = C("070d13");
            Color32 wall = C("141d25");
            Color32 panel = C("202c35");
            Color32 panelEdge = C("344955");
            Color32 floor = C("0e151c");
            Color32 cyan = C("58c8d0");
            Color32 cyanDim = C("245b69");
            Color32 pale = C("9db5b5");
            Color32 red = C("bd3f51");
            Color32 amber = C("b5864d");
            Color32 paper = C("a9a493");

            c.Rect(ox, 0, SceneWidth, SceneHeight, night);

            // Exterior is confined to a built-in left window, giving Mert a different spatial rhythm.
            c.Rect(ox, 19, 137, 142, exterior);
            c.Dither(ox + 7, 22, 125, 132, C("214558"), 53, 2);
            for (int i = 0; i < 8; i++)
            {
                int x = ox - 10 + i * 23;
                int w = 18 + i % 3 * 5;
                int top = 55 + i * 17 % 52;
                c.SteppedBuilding(x, top, w, 160 - top, exteriorMid, C("24485b"), 91 + i);
                c.WindowCluster(x + 4, top + 13, w - 8, 138 - top, cyanDim, C("7d6e54"), 133 + i, 10);
            }
            c.Frame(ox, 13, 143, 154, 7, ink, new Color32(0, 0, 0, 0));
            c.Rect(ox + 136, 18, 8, 146, C("263640"));
            c.Rect(ox + 139, 20, 3, 142, C("455965"));
            c.Rect(ox + 4, 156, 137, 9, C("344752"));
            c.Rect(ox + 6, 165, 133, 5, ink);

            // Clinical architecture: integrated service grid, recessed panels and cable channels.
            c.Rect(ox + 144, 14, 336, 151, wall);
            c.Rect(ox + 151, 21, 322, 5, panelEdge);
            c.Rect(ox + 151, 26, 322, 4, C("0e151b"));
            c.Rect(ox + 157, 38, 298, 6, C("36505d"));
            c.Rect(ox + 157, 44, 298, 3, ink);
            for (int i = 0; i < 6; i++)
            {
                int x = ox + 153 + i * 52;
                int top = 54 + (i % 2) * 7;
                c.Frame(x, top, 47, 92, 3, C("0c1319"), i % 2 == 0 ? panel : C("1b2730"));
                c.Rect(x + 5, top + 5, 37, 3, C("2b3d48"));
                c.Rect(x + 6, top + 81, 13, 3, i == 4 ? C("68303a") : C("204c59"));
                c.Pixel(x + 39, top + 82, i == 3 ? C("85343f") : C("42939e"));
                if (i % 2 == 1)
                {
                    c.Rect(x + 39, top + 17, 2, 47, C("121b21"));
                }
            }
            c.Rect(ox + 149, 145, 316, 8, C("10171d"));
            c.Rect(ox + 155, 147, 304, 2, C("50616a"));

            // Floor: darker metal tile field with seams and controlled reflections.
            c.Rect(ox, 170, SceneWidth, 100, floor);
            c.Rect(ox, 166, SceneWidth, 6, C("354750"));
            c.Rect(ox, 172, SceneWidth, 4, C("090e13"));
            for (int y = 216; y < 270; y += 34)
            {
                c.Line(ox, y, ox + 479, y, C("151f27"));
            }
            for (int x = 70; x < 480; x += 120)
            {
                c.Line(ox + 242, 178, ox + x, 269, C("17242c"));
            }
            c.Dither(ox + 77, 180, 338, 70, C("20353d"), 61, 1);

            // Left research storage is tall, modular and integrated rather than domestic shelving.
            c.Shadow(ox + 29, 174, 91, 34);
            c.Frame(ox + 18, 61, 94, 108, 5, ink, C("202d35"));
            c.Rect(ox + 24, 68, 82, 7, C("3d5360"));
            for (int row = 0; row < 5; row++)
            {
                int y = 81 + row * 17;
                c.Frame(ox + 25, y, 79, 14, 2, C("111920"), C("2a3942"));
                c.Rect(ox + 77, y + 5, 18, 3, pale);
                c.Rect(ox + 31, y + 5, 4, 4, row == 2 ? red : cyan);
                c.Rect(ox + 39, y + 6, 22 + row * 3, 2, C("52656d"));
            }
            c.Rect(ox + 14, 75, 5, 86, C("425762"));
            c.Line(ox + 108, 79, ox + 131, 96, C("17232a"), 3);
            c.Line(ox + 131, 96, ox + 131, 158, C("17232a"), 3);

            // Main workstation uses a low wide chassis and varied monitor heights.
            c.Shadow(ox + 126, 189, 194, 29);
            c.Polygon(new[]
            {
                P(ox + 116, 67), P(ox + 334, 67), P(ox + 340, 80), P(ox + 334, 150),
                P(ox + 119, 150)
            }, C("0c151c"));
            c.Rect(ox + 122, 72, 206, 3, C("243944"));
            c.Polygon(new[]
            {
                P(ox + 121, 151), P(ox + 307, 151), P(ox + 313, 158), P(ox + 304, 166),
                P(ox + 132, 166), P(ox + 119, 159)
            }, C("465761"));
            c.Rect(ox + 128, 158, 178, 8, C("0d151b"));
            c.Frame(ox + 132, 166, 42, 34, 4, ink, C("25343b"));
            c.Frame(ox + 261, 166, 42, 34, 4, ink, C("25343b"));
            c.Rect(ox + 140, 177, 26, 3, C("607178"));
            c.Rect(ox + 269, 177, 26, 3, C("607178"));

            DrawMonitor(c, ox + 128, 94, 48, 48, C("173a46"), false, 1);
            DrawMonitor(c, ox + 179, 77, 55, 65, C("3a96a3"), false, 2);
            DrawMonitor(c, ox + 237, 89, 51, 53, C("235866"), false, 3);
            DrawMonitor(c, ox + 291, 103, 40, 39, C("382731"), true, 4);
            c.Rect(ox + 201, 142, 9, 9, C("283840"));
            c.Rect(ox + 252, 142, 9, 9, C("283840"));
            c.Polygon(new[]
            {
                P(ox + 174, 148), P(ox + 267, 148), P(ox + 277, 155), P(ox + 165, 155)
            }, C("506269"));
            for (int key = 0; key < 16; key++)
            {
                c.Rect(ox + 177 + (key % 8) * 10, 150 + (key / 8) * 3, 5, 1,
                    key == 13 ? red : pale);
            }
            c.Rect(ox + 277, 145, 18, 8, C("2b3940"));
            c.Pixel(ox + 281, 148, cyan);
            c.Pixel(ox + 286, 148, amber);

            // Controlled cable channels become disturbed loose runs near the floor.
            c.Rect(ox + 147, 49, 5, 96, C("111920"));
            c.Rect(ox + 151, 50, 2, 94, C("43545c"));
            c.Line(ox + 149, 61, ox + 181, 74, C("17242b"), 2);
            c.Line(ox + 197, 166, ox + 218, 194, C("081017"), 3);
            c.Line(ox + 218, 194, ox + 257, 190, C("081017"), 3);
            c.Line(ox + 257, 190, ox + 278, 207, C("14242b"), 2);
            c.Line(ox + 165, 166, ox + 151, 205, C("0a1218"), 2);

            // Analysis micro-scene: tray, reader, lamp, tools and damaged implant.
            c.Shadow(ox + 320, 194, 80, 25);
            c.Polygon(new[]
            {
                P(ox + 313, 82), P(ox + 408, 82), P(ox + 411, 193), P(ox + 315, 193)
            }, C("0a1218"));
            c.Frame(ox + 322, 128, 78, 64, 4, ink, C("1b272d"));
            c.Rect(ox + 329, 136, 64, 5, C("536b70"));
            c.Rect(ox + 329, 141, 64, 3, C("10171b"));
            c.Frame(ox + 337, 148, 47, 19, 3, C("0b1217"), C("526466"));
            c.Polygon(new[]
            {
                P(ox + 352, 153), P(ox + 369, 153), P(ox + 375, 158), P(ox + 366, 163),
                P(ox + 350, 160), P(ox + 346, 156)
            }, C("9bbfc0"));
            c.Line(ox + 357, 154, ox + 369, 162, C("d45159"), 1);
            c.Pixel(ox + 371, 157, C("f0a766"));
            c.Rect(ox + 389, 147, 4, 20, C("7c684a"));
            c.Rect(ox + 392, 147, 3, 20, C("5f3d2d"));
            c.Line(ox + 328, 126, ox + 342, 99, C("53646a"), 4);
            c.Line(ox + 342, 99, ox + 370, 90, C("53646a"), 4);
            c.Polygon(new[]
            {
                P(ox + 360, 86), P(ox + 384, 86), P(ox + 379, 96), P(ox + 365, 96)
            }, C("b7d4d2"));
            c.Rect(ox + 362, 96, 19, 3, C("83d8d8"));
            c.Dither(ox + 334, 91, 56, 78, C("4ac0c3", 74), 17, 0);
            c.Polygon(new[]
            {
                P(ox + 323, 190), P(ox + 402, 190), P(ox + 393, 201), P(ox + 334, 201)
            }, C("18353b", 135));
            c.Rect(ox + 309, 151, 12, 8, paper);
            c.Line(ox + 311, 154, ox + 319, 154, C("5b5e58"));
            c.Rect(ox + 302, 145, 7, 14, C("4d5a58"));

            // Door and integrated status system anchor the far right.
            c.Shadow(ox + 409, 171, 64, 24);
            c.Frame(ox + 407, 61, 65, 111, 6, ink, C("202a31"));
            c.Frame(ox + 417, 72, 43, 88, 3, C("40515b"), C("182128"));
            c.Rect(ox + 414, 78, 4, 72, C("a03a48"));
            c.Rect(ox + 419, 81, 2, 66, C("4d2630"));
            c.Rect(ox + 445, 116, 13, 4, pale);
            c.Frame(ox + 466, 77, 10, 35, 2, ink, C("283942"));
            c.Rect(ox + 469, 84, 4, 8, red);
            c.Rect(ox + 469, 97, 4, 4, cyan);

            // Evidence belongs on surfaces; it is not a marker layer.
            c.Polygon(new[]
            {
                P(ox + 287, 153), P(ox + 306, 151), P(ox + 308, 161), P(ox + 289, 163)
            }, C("80745e"));
            c.Rect(ox + 290, 154, 6, 5, C("4c4540"));
            c.Polygon(new[]
            {
                P(ox + 311, 152), P(ox + 329, 154), P(ox + 327, 165), P(ox + 309, 162)
            }, paper);
            c.Line(ox + 313, 157, ox + 324, 159, red);

            // A fallen chair and displaced module break the controlled grid.
            c.Polygon(new[]
            {
                P(ox + 259, 173), P(ox + 279, 169), P(ox + 285, 181), P(ox + 268, 187)
            }, C("243039"));
            c.Line(ox + 269, 185, ox + 252, 210, C("0b1218"), 4);
            c.Line(ox + 280, 181, ox + 302, 202, C("0b1218"), 4);
            c.Line(ox + 257, 173, ox + 247, 151, C("354650"), 5);
            c.Frame(ox + 116, 151, 22, 15, 2, ink, C("34454b"));
            c.Rect(ox + 120, 155, 4, 4, red);

            // Foreground framing is architectural, partial, and non-obstructive.
            c.Polygon(new[]
            {
                P(ox, 0), P(ox + 9, 0), P(ox + 9, 270), P(ox, 270)
            }, C("03070d"));
            c.Polygon(new[]
            {
                P(ox + 470, 0), P(ox + 480, 0), P(ox + 480, 270), P(ox + 465, 270),
                P(ox + 465, 213), P(ox + 470, 213)
            }, C("03070d"));
            c.Line(ox + 6, 39, ox + 56, 39, C("14212a"), 4);
            c.Line(ox + 458, 24, ox + 450, 93, C("23343e"), 3);

            for (int i = 0; i < 9; i++)
            {
                int x = ox + 13 + (i * 47 % 121);
                int y = 28 + (i * 59 % 122);
                if (i % 3 == 0)
                {
                    continue;
                }
                c.Line(x, y, x - 1, y + 3 + i % 4, C("718b98", 82));
            }
        }

        private static void DrawMonitor(PixelCanvas c, int x, int y, int width, int height,
            Color32 screen, bool warning, int seed)
        {
            c.Frame(x, y, width, height, 4, C("080e13"), C("25323a"));
            c.Rect(x + 6, y + 6, width - 12, height - 12, screen);
            Color32 header = warning
                ? C("9a3c49")
                : seed == 2 ? C("a0e5e4") : seed == 3 ? C("5fa4ad") : C("3e7783");
            Color32 data = warning
                ? C("a04a58")
                : seed == 2 ? C("9cd4d4") : seed == 3 ? C("689ba1") : C("526f77");
            c.Rect(x + 8, y + 8, width - 16, 3, header);
            for (int row = 0; row < 4; row++)
            {
                int available = Math.Max(6, width - 19);
                int lineWidth = 6 + ((row * 13 + seed * 7) % available);
                c.Rect(x + 9, y + 16 + row * 6, lineWidth, 2,
                    warning && row == 2 ? C("a33e4d") : data);
            }
            c.Rect(x + width / 2 - 3, y + height, 6, 8, C("182229"));
        }

        private static Texture2D GetOrCreateTexture(string path, int width, int height, string name)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
                {
                    name = name,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0
                };
                AssetDatabase.CreateAsset(texture, path);
            }
            else if (texture.width != width || texture.height != height)
            {
                texture.Reinitialize(width, height, TextureFormat.RGBA32, false);
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.anisoLevel = 0;
            return texture;
        }

        private static void EnsureSprite(Texture2D texture, string assetPath, string name, Rect rect, float ppu)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .FirstOrDefault(item => item.name == name);
            if (sprite != null)
            {
                return;
            }

            sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), ppu, 0,
                SpriteMeshType.FullRect, Vector4.zero, false);
            sprite.name = name;
            AssetDatabase.AddObjectToAsset(sprite, texture);
            EditorUtility.SetDirty(sprite);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }
                current = next;
            }
        }

        private static Vector2Int P(int x, int y) => new(x, y);

        private static Color32 C(string hex, byte alpha = 255)
        {
            if (!ColorUtility.TryParseHtmlString("#" + hex, out Color color))
            {
                throw new ArgumentException($"Invalid color '{hex}'.", nameof(hex));
            }
            Color32 value = color;
            value.a = alpha;
            return value;
        }

        private sealed class PixelCanvas
        {
            private readonly int _width;
            private readonly int _height;

            internal PixelCanvas(int width, int height)
            {
                _width = width;
                _height = height;
                Pixels = new Color32[width * height];
            }

            internal Color32[] Pixels { get; }

            internal void Clear(Color32 color)
            {
                Array.Fill(Pixels, color);
            }

            internal void Pixel(int x, int y, Color32 color)
            {
                if (x < 0 || x >= _width || y < 0 || y >= _height)
                {
                    return;
                }

                int index = (_height - 1 - y) * _width + x;
                if (color.a >= 255)
                {
                    Pixels[index] = color;
                    return;
                }

                Color32 under = Pixels[index];
                float alpha = color.a / 255f;
                Pixels[index] = new Color32(
                    (byte)Mathf.RoundToInt(Mathf.Lerp(under.r, color.r, alpha)),
                    (byte)Mathf.RoundToInt(Mathf.Lerp(under.g, color.g, alpha)),
                    (byte)Mathf.RoundToInt(Mathf.Lerp(under.b, color.b, alpha)),
                    (byte)Mathf.Max(under.a, color.a));
            }

            internal void Rect(int x, int y, int width, int height, Color32 color)
            {
                for (int py = Math.Max(0, y); py < Math.Min(_height, y + height); py++)
                {
                    for (int px = Math.Max(0, x); px < Math.Min(_width, x + width); px++)
                    {
                        Pixel(px, py, color);
                    }
                }
            }

            internal void Frame(int x, int y, int width, int height, int thickness, Color32 border, Color32 fill)
            {
                if (fill.a > 0)
                {
                    Rect(x + thickness, y + thickness, Math.Max(0, width - thickness * 2),
                        Math.Max(0, height - thickness * 2), fill);
                }
                Rect(x, y, width, thickness, border);
                Rect(x, y + height - thickness, width, thickness, border);
                Rect(x, y, thickness, height, border);
                Rect(x + width - thickness, y, thickness, height, border);
            }

            internal void Shadow(int x, int y, int width, int height)
            {
                Polygon(new[]
                {
                    P(x, y), P(x + width, y), P(x + width - 8, y + height), P(x + 8, y + height)
                }, C("05090e", 210));
            }

            internal void Dither(int x, int y, int width, int height, Color32 color, int modulus, int phase)
            {
                for (int py = y; py < y + height; py++)
                {
                    for (int px = x; px < x + width; px++)
                    {
                        uint hash = unchecked((uint)(px * 374761393 + py * 668265263 + phase * 1442695041));
                        hash = (hash ^ (hash >> 13)) * 1274126177u;
                        hash ^= hash >> 16;
                        if (hash % (uint)modulus == 0u)
                        {
                            Pixel(px, py, color);
                        }
                    }
                }
            }

            internal void Line(int x0, int y0, int x1, int y1, Color32 color, int thickness = 1)
            {
                int dx = Math.Abs(x1 - x0);
                int sx = x0 < x1 ? 1 : -1;
                int dy = -Math.Abs(y1 - y0);
                int sy = y0 < y1 ? 1 : -1;
                int error = dx + dy;
                while (true)
                {
                    Rect(x0 - thickness / 2, y0 - thickness / 2, thickness, thickness, color);
                    if (x0 == x1 && y0 == y1)
                    {
                        break;
                    }
                    int doubled = error * 2;
                    if (doubled >= dy)
                    {
                        error += dy;
                        x0 += sx;
                    }
                    if (doubled <= dx)
                    {
                        error += dx;
                        y0 += sy;
                    }
                }
            }

            internal void Polygon(Vector2Int[] points, Color32 color)
            {
                if (points == null || points.Length < 3)
                {
                    return;
                }

                int minY = points.Min(point => point.y);
                int maxY = points.Max(point => point.y);
                var intersections = new int[points.Length];
                for (int y = minY; y <= maxY; y++)
                {
                    int count = 0;
                    int previous = points.Length - 1;
                    for (int current = 0; current < points.Length; current++)
                    {
                        Vector2Int a = points[current];
                        Vector2Int b = points[previous];
                        if ((a.y < y && b.y >= y) || (b.y < y && a.y >= y))
                        {
                            intersections[count++] = a.x + (y - a.y) * (b.x - a.x) / (b.y - a.y);
                        }
                        previous = current;
                    }

                    Array.Sort(intersections, 0, count);
                    for (int index = 0; index + 1 < count; index += 2)
                    {
                        Rect(intersections[index], y, intersections[index + 1] - intersections[index] + 1, 1, color);
                    }
                }
            }

            internal void SteppedBuilding(int x, int y, int width, int height, Color32 body, Color32 edge, int seed)
            {
                int step = 3 + seed % 5;
                Polygon(new[]
                {
                    P(x, y + step), P(x + step, y + step), P(x + step, y),
                    P(x + width - step * 2, y), P(x + width - step * 2, y + step / 2),
                    P(x + width, y + step / 2), P(x + width, y + height), P(x, y + height)
                }, body);
                Rect(x + 2, y + step + 3, 2, Math.Max(1, height - step - 7), edge);
                Rect(x + 4, y + height - 4, Math.Max(1, width - 6), 3, C("09131e", 150));
            }

            internal void WindowCluster(int x, int y, int width, int height, Color32 cool, Color32 warm, int seed, int spacing)
            {
                if (width <= 3 || height <= 3)
                {
                    return;
                }

                int row = 0;
                for (int py = y; py < y + height; py += spacing)
                {
                    int column = 0;
                    for (int px = x; px < x + width; px += spacing - 1)
                    {
                        int hash = seed + row * 17 + column * 29;
                        if (Mathf.Abs(hash * 13) % 11 < 2)
                        {
                            Color32 color = hash % 17 == 0 ? warm : cool;
                            Rect(px, py, 3 + Math.Abs(hash) % 2, 2 + Math.Abs(hash / 3) % 2, color);
                        }
                        column++;
                    }
                    row++;
                }
            }
        }
    }
}
