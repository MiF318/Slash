using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.Design.AxImporter;

namespace Slash
{
    public class Game
    {
        private Graphics Graphics;
        private Action Invalidate;
        public readonly int TPS = 10;

        public List<AllActions> UserActions;

        public Level CurrentLevel;

        public Slasher Slasher;

        public Statuses Status;

        public bool EndLevel = false;

        public bool NextLevel = false;
        public bool PrevLevel = false;

        public Game(Graphics graphics, Action invalidate)
        {
            Graphics = graphics;
            Invalidate = invalidate;
            UserActions = new List<AllActions>();
        }

        public void Start(System.Windows.Forms.Timer timer1)
        {
            GenerateLevels();

            CurrentLevel = Levels.Level;

            Start();

            InitTimer(timer1);

            Invalidate();
        }


        public void OnPaintUpdate(Graphics graphics)
        {
            Graphics = graphics;

            CurrentLevel.Display(Graphics);

            Entity.DisplayAll(Graphics);

            DisplayOthers();
        }
        private void DisplayOthers()
        {
            if (!CurrentLevel.SpawnEnemies)
                return;

            var currentX = 20;
            Graphics.DrawImage(Textures.Other.PlashkaForGribs, 10, 10, CurrentLevel.CountTotemToWin * 40 + 20, 100);
            for (int i = 0; i < CurrentLevel.Hole.KilledTotems.Count; i++)
            {
                Graphics.DrawImage(Textures.Entities.GetTexture(Entity.Types.Totem), currentX, 20, 1 * CurrentLevel.BlockSize, 2 * CurrentLevel.BlockSize);
                currentX += 1 * CurrentLevel.BlockSize;
            }
        }

        private void InitTimer(System.Windows.Forms.Timer timer1)
        {
            timer1.Interval = TPS;
            timer1.Tick += new EventHandler(update);
            timer1.Start();
        }

        private void update(object sender, EventArgs e)
        {
            CheckGameStatus();

            UserActions = new List<AllActions>();
            EndLevel = false;

            Invalidate();
        }

        private void GameTick()
        {
            CurrentLevel.Tick(this);

            Slasher.OneTimeActions.ActionHandling(UserActions);

            Entity.TickForAll(this);
        }

        private void CheckGameStatus()
        {
            if (Status != Statuses.Pause)
            {
                GameTick();
            }
            else
            {
                Slasher.OneTimeActions = new Actions();
            }

            if (Status == Statuses.Loose)
            {

                //Мб подождать сколько то
                Restart();
            }

            else if (NextLevel)
            {
                Next();
                NextLevel = false;
            }
                
            else if (PrevLevel)
            {
                Previous();
                PrevLevel = false;
            }

            else if (Status == Statuses.Win && EndLevel && CurrentLevel.GodPortal.Active)
            {
                foreach (var segment in Slasher.Hitbox.HitboxSides.Values)
                {
                    foreach (var segmentPoint in segment.GetSegmentPoints(2))
                    {
                        if (segmentPoint.CheckLocationPointInHitbox(CurrentLevel.GodPortal.Hitbox))
                        {
                            // Мб немного подождать, показать статистику

                            Next();
                        }
                    }
                }
            }
            else if (Status == Statuses.CheatWin)
                Next();
        }
        private void Start()
        {
            Entity.All.Clear();

            Slasher = Slasher.Spawn(CurrentLevel.SpawnPoint);

            Status = Statuses.Play;
        }
        private void Restart()
        {
            CurrentLevel = Levels.Restart();

            Start();
        }
        private void Next()
        {
            CurrentLevel = Levels.Next();

            Start();
        }
        private void Previous()
        {
            CurrentLevel = Levels.Previous();

            Start();
        }

        public void GenerateLevels()
        {
            var allLevels = new Level[]
            {
                //48 x 27
                new Level(Textures.BG.GetBG(Locations.MagicForest), 48, 27, 40, Maps.Camp, 1, 1, 0, false),

                new Level(Textures.BG.GetBG(Locations.MagicForest), 48, 27, 40, Maps.Map1, 20, 70 * TPS, 5, true),
                new Level(Textures.BG.GetBG(Locations.MagicForest), 48, 27, 40, Maps.Map2, 18, 60 * TPS, 8, true),
                new Level(Textures.BG.GetBG(Locations.MagicForest), 48, 27, 40, Maps.Map3, 15, 60 * TPS, 7, true),
                new Level(Textures.BG.GetBG(Locations.MagicForest), 48, 27, 40, Maps.Map4, 20, 75 * TPS, 5, true),

                new Level(Textures.BG.GetBG(Locations.LavaWastelands), 48, 27, 40, Maps.Map5, 10, 35 * TPS, 6, true),
                new Level(Textures.BG.GetBG(Locations.LavaWastelands), 48, 27, 40, Maps.Map6, 10, 35 * TPS, 5, true),
                new Level(Textures.BG.GetBG(Locations.LavaWastelands), 48, 27, 40, Maps.Map7, 10, 35 * TPS, 7, true),
                new Level(Textures.BG.GetBG(Locations.LavaWastelands), 48, 27, 40, Maps.Map8, 10, 55 * TPS, 5, true),
            };

            Levels.Generate(allLevels);
        }

        public enum Statuses
        {
            Play,
            Loose,
            Win,
            CheatWin,
            Pause
        }
    }

    public interface IGameTick
    {
        public void Tick(Game game);
        public void Display(Graphics graphics);
    }
}
