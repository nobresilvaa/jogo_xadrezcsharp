using System;
using System.Collections.Generic;
using System.Threading;
using tabuleiro;
using Xadrez;

namespace xadrez
{
    internal class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }

        private HashSet<Peca> pecas;

        private HashSet<Peca> capturadas;

        public bool xeque { get; private set; }

        public  Peca vuneravelEnPassant;

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            xeque = false;
            vuneravelEnPassant = null;
            turno = 1;
            jogadorAtual = Cor.branca;
            terminada = false;
            colocarPecas();

        }

        public Peca executarMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQntMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if ( pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            ////jogada especial roque pequeno
            if(p is Rei && destino.coluna== origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQntMovimentos();
                tab.colocarPeca(T, destinoT);
            }

            ////jogada especial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQntMovimentos();
                tab.colocarPeca(T, destinoT);
            }

            //jogada especial en-passant
            if(p is Peao)
            {
                if(origem.coluna!= destino.coluna && pecaCapturada== null)
                {
                    Posicao posP;
                    if(p.cor== Cor.branca)
                    {
                        posP = new Posicao(destino.linha +1,destino.coluna);
                    }
                    else
                    {
                        posP = new Posicao(destino.linha -1,destino.coluna);
                    }
                    pecaCapturada = tab.retirarPeca(posP);
                    capturadas.Add (pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem,Posicao destino,Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimento();
            if(pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);


            ////jogada especial roque pequeno
            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.decrementarQteMovimento();
                tab.colocarPeca(T, destinoT);
            }


            ////jogada especial roque grande
            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.decrementarQteMovimento();
                tab.colocarPeca(T, destinoT);
            }

            //jogada especial en-passant
            if(p is Peao)
            {
                if(origem.coluna!=destino.coluna && pecaCapturada == vuneravelEnPassant)
                {
                    Peca peao = tab.retirarPeca(destino);
                    Posicao posP;
                    if(p.cor == Cor.branca){
                        posP = new Posicao(3,destino.coluna);
                    }

                    else
                    {
                        posP = new Posicao(4,destino.coluna);
                    }
                    tab.colocarPeca(peao, posP);
                }
            }

        }

       

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executarMovimento(origem, destino);

            if (estaEmXaque(jogadorAtual))
            {
                desfazMovimento(origem, destino,pecaCapturada);
                throw new TabuleiroExcecption("Você não pode se colocar em xeque!");
            }

            Peca p = tab.peca(destino);

            //#jogada especial promocao
            if(p is Peao)
            {
                if((p.cor==Cor.branca && destino.linha== 00)||p.cor ==Cor.preta && destino.linha == 7)
                {
                    p = tab.retirarPeca(destino);
                    pecas.Remove(p);
                    Peca dama = new Dama(tab, p.cor);
                    tab.colocarPeca(dama, destino);
                    pecas.Add(dama);
                }
            }


            if (estaEmXaque(adversaria (jogadorAtual)))
            {
                xeque = true; 
            }

            else
            {
                xeque= false;
            }

            if (testeXequeMate(adversaria(jogadorAtual)))
            {
                terminada = true;
            }

            else { 
            turno++;
            mudaJogador();
            }

            //jogada especial en-passant
            
            if (p is  Peao && (destino.linha== origem.linha - 2 || destino.linha == origem.linha + 2))
            {
                vuneravelEnPassant = p;
            }

            else
            {
                vuneravelEnPassant= null;
            }

        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if(tab.peca(pos) == null)
            {
                throw new TabuleiroExcecption("Não existe peça na posiçõa de origem escolhida!");
            }

            if(jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroExcecption(" A peça escolhida não é sua!");
            }

            if(!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroExcecption("Não há movimentos possiveis para a peça de origem escolhida! ");
            }
        }

        public void validarPosicaoDestino(Posicao origem, Posicao destino) 
        {
            if(!tab.peca(origem).movimentoPossivel(destino))
            {
                throw new TabuleiroExcecption("Posição de destino invalida!");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.branca)
            {
                jogadorAtual = Cor.preta;
            }

            else
            {
                jogadorAtual = Cor.branca;
            }
        }

        public HashSet<Peca>pecasCapturadas(Cor cor)
        {
            HashSet<Peca>aux = new HashSet<Peca>();
            foreach(Peca x in capturadas)
            {
                if (x.cor == cor)
                    aux.Add(x);
            }
            return aux;
        }

        public HashSet<Peca>pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in pecas)
            {
                if(x.cor == cor)
                {
                    aux.Add(x);
                }
             
              
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        private Cor adversaria (Cor cor)
        {
            if (cor == Cor.branca)
            {
                return Cor.preta;
            }

            else{
                return Cor.branca;
            }
        }

        private Peca rei (Cor cor)
        {
            foreach(Peca x in pecasEmJogo(cor))
            {
                if (x is Rei ) 
                {
                    return x;
                }
            }

            return null;
        }

        public bool estaEmXaque(Cor cor)
        {
            Peca R= rei(cor);

            if(R == null) 
            {
                throw new TabuleiroExcecption("Não existe rei da cor " + cor + "no tabuleiro!");
            }

            foreach(Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();  
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor) 
        {
            if (!estaEmXaque(cor))
            {
                return false;
            }
            foreach(Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for(int i = 0; i < tab.linhas; i++)
                {
                    for(int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i,j);
                            Peca pecaCapturada = executarMovimento(origem, destino);
                            bool testeXeque = estaEmXaque(cor);
                            desfazMovimento(origem,destino, pecaCapturada);
                            if (!testeXeque) {
                            return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void ColocarNovaPeca(char coluna, int linha,Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }


        private void colocarPecas()
        {

            ColocarNovaPeca('a',1,new Torre(tab, Cor.branca));

            ColocarNovaPeca('b', 1, new Cavalo(tab, Cor.branca));

           ColocarNovaPeca('c', 1, new Bispo(tab, Cor.branca));

            ColocarNovaPeca('d', 1, new Dama(tab, Cor.branca));

           ColocarNovaPeca('e',1,new Rei(tab, Cor.branca,this));

            ColocarNovaPeca('f', 1, new Bispo(tab, Cor.branca));

            ColocarNovaPeca('g', 1, new Cavalo(tab, Cor.branca));

            ColocarNovaPeca('h', 1, new Torre(tab, Cor.branca));


            ColocarNovaPeca('a', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('b', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('c', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('d', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('e', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('f', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('g', 2, new Peao(tab, Cor.branca, this));
            ColocarNovaPeca('h', 2, new Peao(tab, Cor.branca, this));
            //////////////////////////////////////////////////////////


            ColocarNovaPeca('a', 8, new Torre(tab, Cor.preta));

            ColocarNovaPeca('b', 8, new Cavalo(tab, Cor.preta));

            ColocarNovaPeca('c', 8, new Bispo(tab, Cor.preta));

            ColocarNovaPeca('d', 8, new Dama(tab, Cor.preta));

            ColocarNovaPeca('e', 8, new Rei(tab, Cor.preta,this));

           ColocarNovaPeca('f', 8, new Bispo(tab, Cor.preta));

            ColocarNovaPeca('g', 8, new Cavalo(tab, Cor.preta));

            ColocarNovaPeca('h', 8, new Torre(tab, Cor.preta));
           

            ColocarNovaPeca('a', 7, new Peao(tab, Cor.preta,this));
            ColocarNovaPeca('b', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('c', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('d', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('e', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('f', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('g', 7, new Peao(tab, Cor.preta, this));
            ColocarNovaPeca('h', 7, new Peao(tab, Cor.preta, this));
        }
    }
}
