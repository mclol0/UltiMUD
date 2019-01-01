using System;
using System.Reflection;

namespace Ultimud
{
    [Serializable]
    [Obfuscation(Exclude = true)]
    public class CommandAction : Action
    {
        string cmd_;
        Damdai.Timer timer_;
        User user_;

        public CommandAction(string command, int delay, User user)
            : base(user.Character)
        {
            this.cmd_ = command;
            this.timer_ = new Damdai.Timer(new TimeSpan(0, 0, delay));
            this.user_ = user;
        }
        public CommandAction(string command, int delay, Npc npc) : this(command, delay, new User(npc)) { }

        [Obfuscation(Exclude = false)]
        public override void Start()
        {
            this.timer_.Reset();
            //this.Update();
        }

        [Obfuscation(Exclude = false)]
        public override bool Stop(bool silent)
        {
            this.object_.RemoveAction(this);
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override bool Wait()
        {
            this.paused_ = true;
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Go()
        {
            this.paused_ = false;
        }

        [Obfuscation(Exclude = false)]
        public override bool Finish()
        {
            this.object_.RemoveAction(this);
            string[] commands = this.cmd_.Split(';');
            foreach (string command in commands)
            {
                try
                {
                    this.user_.Command = command;
                    Server.Instance.ProcessCommand(this.user_, true);
                }
                catch (AbortCommandException)
                {
                    break;
                }
                finally
                {
                    this.user_.FinishInput();
                }
            }
            return true;
        }

        [Obfuscation(Exclude = false)]
        public override void Update()
        {
            if (this.paused_)
                return;

            if (!this.timer_.Update())
                this.Finish();
        }
    }
}
