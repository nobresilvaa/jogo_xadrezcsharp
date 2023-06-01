using System;
using xadrez;
namespace tabuleiro
{
    internal class Program
    {   
        static void Main(string[] args)
        {

            try
            {
                PartidaDeXadrez partida = new PartidaDeXadrez();

                while(!partida.terminada)
                {

                    try
                    {

                        Console.Clear();
                        Tela.imprimirPartida(partida);

                        Console.WriteLine();
                        Console.Write("Origem: ");
                        Posicao origem = Tela.lerPosicaoXadrez().toPosicao();
                        partida.validarPosicaoDeOrigem(origem);

                        bool[,] posicoePossiveis = partida.tab.peca(origem).movimentosPossiveis();


                        Console.Clear();
                        Tela.imprimirTabuleiro(partida.tab, posicoePossiveis);

                        Console.WriteLine();
                        Console.Write("Destino: ");
                        Posicao destino = Tela.lerPosicaoXadrez().toPosicao();
                        partida.validarPosicaoDestino(origem, destino); 

                        partida.realizaJogada(origem, destino);
                    }
                    catch(TabuleiroExcecption e)
                    {
                        Console.WriteLine(e.Message);
                        Console.ReadLine();
                    }
                }

                Console.Clear();
                Tela.imprimirPartida(partida);

                }
           catch (TabuleiroExcecption e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
            
        }
    }
}
