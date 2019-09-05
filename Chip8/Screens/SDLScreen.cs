using System;
using System.Threading.Tasks;

using SDL2;

namespace Chip8.Screens
{
    class SDLScreen : IScreen, IDisposable
    {
        private IntPtr window;
        private IntPtr renderer;

        private readonly State state;
        private readonly int scale;

        public Action<string> Log = msg => { };

        public SDLScreen(State state, int scale = 10)
        {
            this.state = state;
            this.scale = scale;
            var width = scale * 64;
            var height = scale * 32;

            Task.Run(() =>
            {
                SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
                if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
                {
                    throw new InvalidOperationException($"Unable to initialize SDL. Error: { SDL.SDL_GetError()}");
                }

                window = SDL.SDL_CreateWindow("CHIP-8", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, width, height, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
                if (window == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Unable to create a window. SDL. Error: { SDL.SDL_GetError()}");
                }

                renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

                bool quit = false;
                SDL.SDL_Event e;

                while (!quit)
                {
                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        switch (e.type)
                        {
                            case SDL.SDL_EventType.SDL_DROPFILE:
                                var file = SDL.UTF8_ToManaged(e.drop.file, true);
                                Log($"Dropped file {file}");
                                break;

                            case SDL.SDL_EventType.SDL_QUIT:
                                quit = true;
                                break;

                            case SDL.SDL_EventType.SDL_KEYDOWN:

                                switch (e.key.keysym.sym)
                                {
                                    case SDL.SDL_Keycode.SDLK_q:
                                        quit = true;
                                        break;
                                }

                                break;
                        }
                    }
                }
            });
        }

        public void Update()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 219, 218, 177, 0);
            SDL.SDL_RenderClear(renderer);

            SDL.SDL_SetRenderDrawColor(renderer, 44, 81, 70, 255);

            var y = 0;
            foreach (var row in state.ScreenBuffer)
            {
                var x = 0;
                foreach (var bit in Convert.ToString((long)row, 2).PadLeft(64, '0'))
                {
                    if (bit != '1')
                    {
                        for (var sy = 0; sy < scale; sy++)
                        {
                            for (var sx = 0; sx < scale; sx++)
                            {
                                SDL.SDL_RenderDrawPoint(renderer, x + sx, y + sy);
                            }
                        }
                    }

                    x += scale;
                }

                y += scale;
            }

            SDL.SDL_RenderPresent(renderer);
        }

        public void Dispose()
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
