using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Effects;

namespace LoadRetimer {
    class BrightnessEffect : ShaderEffect {

        private static PixelShader m_shader = new PixelShader() {
            UriSource = MakePackUri("Shader/brightness.ps")
        };

        public static Uri MakePackUri(string relativeFile) {
            Assembly a = typeof(BrightnessEffect).Assembly;
            string assemblyShortName = a.ToString().Split(',')[0];
            string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }

        public Brush Input {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BrightnessEffect), 0);

        public BrightnessEffect() {
            PixelShader = m_shader;
            UpdateShaderValue(InputProperty);
        }
    }
}
