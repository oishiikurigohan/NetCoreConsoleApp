using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NetCoreConsoleApp
{
    public class Game : GameWindow
    {
        // VBOの識別番号 [0]:位置情報 [1]:色情報
        private int[] vbo = new int[2];

        // ShaderProgramの識別番号
        private int shaderProgram;

        // 三角形の頂点データ
        private float[] vertexPosition = {
            0.0f,  0.5f,
            0.4f, -0.25f,
           -0.4f, -0.25f,
        };

        // 三角形の各頂点の色情報データ(RGBA)
        private float[] vertexColor = {
            1.0f, 0.0f, 0.0f, 1.0f,
            0.0f, 1.0f, 0.0f, 1.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
        };

        // 色反転フラグ
        private bool flg = true;


        // コンストラクタ
        public Game() : base(400, 400, GraphicsMode.Default, "--- OpenTK Test ---")
        {
            VSync = VSyncMode.On;
        }

        // 画面起動時に呼び出される
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string info;
            int status;

            // VertexShaderの作成
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var sv = new StreamReader("shader.vert");
            GL.ShaderSource(vertexShader, sv.ReadToEnd());
            GL.CompileShader(vertexShader);
            GL.GetShaderInfoLog(vertexShader, out info);
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
            if (status != 1) { throw new Exception(info); }

            // FragmentShaderの作成
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            var sf = new StreamReader("shader.frag");
            GL.ShaderSource(fragmentShader, sf.ReadToEnd());
            GL.CompileShader(fragmentShader);
            GL.GetShaderInfoLog(fragmentShader, out info);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
            if (status != 1) { throw new Exception(info); }

            // ShaderProgramオブジェクトの作成
            this.shaderProgram = GL.CreateProgram();
            GL.AttachShader(this.shaderProgram, vertexShader);
            GL.AttachShader(this.shaderProgram, fragmentShader);

            // Shaderオブジェクトの削除
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // リンク
            GL.LinkProgram(this.shaderProgram);
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);
            GL.GetProgramInfoLog(shaderProgram, out info);
            if (status != 1) { throw new Exception(info); }

            // 使用するShaderProgramを指定
            GL.UseProgram(this.shaderProgram);

            // 各Arrayを有効化
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            // VBO(頂点バッファオブジェクト)を2つ作成
            GL.GenBuffers(2, this.vbo);

            // VBO[0]に位置情報をセットし、VertexShaderの変数を関連づける
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo[0]);
            int size = Marshal.SizeOf(typeof(float)) * this.vertexPosition.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(this.vertexPosition, 0, ptr, this.vertexPosition.Length);
            GL.BufferData(BufferTarget.ArrayBuffer, size, ptr, BufferUsageHint.StaticDraw);
            var attrPosition = GL.GetAttribLocation(shaderProgram, "position");
            GL.EnableVertexAttribArray(attrPosition);
            GL.VertexAttribPointer(attrPosition, 2, VertexAttribPointerType.Float, false, 0, 0);

            // VBO[1]にFragmentShaderの変数を関連づける
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo[1]);
            int attrColor = GL.GetAttribLocation(shaderProgram, "color");
            GL.EnableVertexAttribArray(attrColor);
            GL.VertexAttribPointer(attrColor, 4, VertexAttribPointerType.Float, false, 0, 0);
        }

        // 画面のサイズが変更されたとき
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(ClientRectangle);
        }

        // 画面を更新するときに呼ばれる
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        // 画面が描画されるときに呼ばれる
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            // 指定したRGBA(Aは透明度)でColorバッファを初期化
            GL.ClearColor(0.2f, 0.2f, 0.2f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // VBO[1]に頂点の色情報をセットする
            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vbo[1]);
            int size = Marshal.SizeOf(typeof(float)) * this.vertexColor.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(this.vertexColor, 0, ptr, this.vertexColor.Length);
            GL.BufferData(BufferTarget.ArrayBuffer, size, ptr, BufferUsageHint.StaticDraw);

            // 現在バインドされているバッファから、頂点0番から3個分の頂点データを取ってきて三角形を描画
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            // GPUに全ての描画コマンドを現在のフレームに対して実行するように命じる
            SwapBuffers();

            // メモリ解放
            Marshal.FreeHGlobal(ptr);

            // 三角形の頂点の色情報を変更する
            if (flg)
            {
                vertexColor[0] -= 0.005f;
                vertexColor[1] += 0.005f;
                vertexColor[5] -= 0.005f;
                vertexColor[6] += 0.005f;
                vertexColor[8] += 0.005f;
                vertexColor[10] -= 0.005f;
                if (vertexColor[1] >= 1.0f) { flg = false; }

            } else {
                vertexColor[0] += 0.005f;
                vertexColor[1] -= 0.005f;
                vertexColor[5] += 0.005f;
                vertexColor[6] -= 0.005f;
                vertexColor[8] -= 0.005f;
                vertexColor[10] += 0.005f;
                if (vertexColor[1] <= 0.0f) { flg = true; }
            }
        }

        // 画面終了時に呼び出される
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            // ShaderProgramの削除
            GL.DeleteProgram(shaderProgram);

            // バッファのBindを解除
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // VBOを2つ削除
            GL.DeleteBuffers(2, this.vbo);

            // 各Arrayを無効化
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.ColorArray);
        }
    }
}
