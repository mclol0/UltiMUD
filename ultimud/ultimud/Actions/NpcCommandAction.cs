using System;
using System.Drawing;

namespace Ultimud
{
    [Serializable]
    public class NpcCommandAction : Action
    {
        string command_;
        Damdai.Utils.Timer timer_;

        public NpcCommandAction(string command, int delay, Npc npc)
            : base(npc)
        {
            this.command_ = command;
            this.timer_ = new Damdai.Utils.Timer(new TimeSpan(0, 0, delay));
        }

        public override void Start()
        {
            this.timer_.Reset();
        }

        public override bool Stop()
        {
            this.object_.RemoveAction(this);
            return true;
        }

        public override bool Wait()
        {
            this.paused_ = true;
            return true;
        }

        public override void Go()
        {
            this.paused_ = false;
        }

        public override bool Finish()
        {
            this.object_.RemoveAction(this);
            Npc npc = this.object_ as Npc;
            User user = new User();
            user.Character = npc;
            string[] commands = this.command_.Split(';');
            foreach (string command in commands)
            {
                user.Command = command;

                try
                {
                    Server.Instance.ProcessCommand(user);
                }
                catch (AbortCommandException)
                {
                    break;
                }
            }
            return true;
        }

        public override void Update()
        {
            if (this.paused_)
            {
                return;
            }

            if (!this.timer_.Update())
            {
                this.Finish();
            }
        }
    }
}
