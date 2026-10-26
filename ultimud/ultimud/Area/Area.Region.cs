using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reflection;
using Damdai.TypeConverters;

namespace Ultimud
{
    public partial class Area
    {
        [Serializable]
        [Obfuscation(Exclude = true)]
        public class Region : Layer
        {
            [Description("This script is executed when an object enters this region.\r\n\r\nObjects: layer, actor\r\nReturns: nothing")]
            [Editor(typeof(StringEditor), typeof(UITypeEditor))]
            [Category("Scripts")]
            public string OnEnter
            {
                get
                {
                    string script;
                    if (!this.layerscripts_.TryGetValue(LayerScriptType.OnEnter, out script))
                        this.layerscripts_.Add(LayerScriptType.OnEnter, null);
                    return script;
                }
                set
                {
                    if (this.layerscripts_.ContainsKey(LayerScriptType.OnEnter))
                        this.layerscripts_[LayerScriptType.OnEnter] = value;
                    else
                        this.layerscripts_.Add(LayerScriptType.OnEnter, value);
                }
            }

            [Description("This script is executed when an object leaves this region.\r\n\r\nObjects: layer, actor\r\nReturns: nothing")]
            [Editor(typeof(StringEditor), typeof(UITypeEditor))]
            [Category("Scripts")]
            public string OnLeave
            {
                get
                {
                    string script;
                    if (!this.layerscripts_.TryGetValue(LayerScriptType.OnLeave, out script))
                        this.layerscripts_.Add(LayerScriptType.OnLeave, null);
                    return script;
                }
                set
                {
                    if (this.layerscripts_.ContainsKey(LayerScriptType.OnLeave))
                        this.layerscripts_[LayerScriptType.OnLeave] = value;
                    else
                        this.layerscripts_.Add(LayerScriptType.OnLeave, value);
                }
            }

            [Obfuscation(Exclude = false)]
            public virtual void OnObjectEnter(Object actor)
            {
                if (actor != null && this.StatusEffects != null)
                {
                    foreach (StatusEffect se in this.StatusEffects)
                        se.Apply(actor, this);
                }
                this.DoScript(LayerScriptType.OnEnter, actor);
            }

            [Obfuscation(Exclude = false)]
            public virtual void OnObjectLeave(Object actor)
            {
                this.DoScript(LayerScriptType.OnLeave, actor);
            }
        }
    }
}
