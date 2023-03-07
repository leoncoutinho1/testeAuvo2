# Teste Dev Pleno na Auvo (versão 2)

Nesta versão criei um console application que recebe o caminho dos arquivos .csv e retorna um json com o resumo dos pagamentos. Diferente da versão MVC esse entrega o resumo baseado na data dos arquivos. Eu presumi que nos arquivos haverá marcação de ponto para todos os dias do mês mesmo que o funcionário tenha faltado e, nesse caso, todas as estarão com mesmo valor (resultando em horas trabalhadas = 0). Utilizei o método foreach da classe Parallel e configurei o número máximo de threads de acordo com o número de núcleos da máquina que está executando. Cada arquivo é lido em uma thread e todos são adicionados na lista ao final do processamento.

## License

[MIT](https://choosealicense.com/licenses/mit/)