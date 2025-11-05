using MongoDB.Bson;
using MongoDB.Driver;
// Se a classe ConfiguracaoLimpeza estiver em outra pasta, adicione o using
// using API_ovni.Models; 

namespace API_ovni.Services
{
    public class LimpezaArmazenamentoService
    {
        private readonly IMongoCollection<OvniData> _ovniDataCollection;
        private readonly IMongoCollection<ConfiguracaoLimpeza> _configCollection;

        public LimpezaArmazenamentoService(
            IMongoCollection<OvniData> ovniDataCollection,
            IMongoCollection<ConfiguracaoLimpeza> configCollection)
        {
            _ovniDataCollection = ovniDataCollection;
            _configCollection = configCollection;
        }

        // Verifica o tamanho atual da base
        private async Task<long> ObterTamanhoAtualAsync()
        {
            var stats = await _ovniDataCollection.Database.RunCommandAsync<BsonDocument>(new BsonDocument("collStats", _ovniDataCollection.CollectionNamespace.CollectionName));
            return stats["size"].ToInt64(); 
        }

        // Remove registros antigos até o uso ficar abaixo do limite
        public async Task Limpar(int quantidadeNovosDocumentos)
        {
            var config = await _configCollection
                                    .Find(x => x.Id == "config_adsb")
                                    .FirstOrDefaultAsync();

            if (config == null)
            {
                Console.WriteLine("ERRO: Documento de configuração 'config_adsb' não encontrado.");
                return;
            }



                    /*
        /// <summary>
        /// Executa a rotina de limpeza de armazenamento com base em uma meta de ocupação.
        /// </summary>
        /// <remarks>
        /// Esta lógica é acionada sempre que novos documentos são inseridos.
        /// 
        /// 1. BUSCA CONFIGURAÇÃO: 
        ///    Lê o documento "config_adsb" do banco para obter duas regras:
        ///    - 'limiteMaximoBytes': O tamanho máximo (em bytes) que a coleção pode ter.
        ///    - 'percentualAlvoOcupacao': A meta de ocupação desejada após a limpeza (ex: 0.7 para 70%).
        /// 
        /// 2. VERIFICA O LIMITE:
        ///    Compara o 'tamanhoAtual' da coleção com o 'limiteMaximoBytes'.
        ///    Se o limite não foi atingido, a função é encerrada.
        /// 
        /// 3. CALCULA O "BUFFER" (META INTELIGENTE):
        ///    Se o limite foi atingido, a limpeza é iniciada.
        ///    O objetivo não é apenas abrir espaço, mas criar uma "folga" (buffer).
        ///    - Ex: Se a meta é 70% ('percentualAlvoOcupacao' = 0.7), o percentual a remover será 30% (1.0 - 0.7).
        ///    - Garante a remoção mínima de 10% (Math.Max(0.1, ...)).
        /// 
        /// 4. CALCULA A QUANTIDADE:
        ///    Calcula a quantidade de documentos a remover com base no percentual (ex: 30% do total).
        ///    Compara esse número com a 'quantidadeNovosDocumentos' que estão chegando.
        ///    A limpeza removerá o MAIOR desses dois valores (Math.Max), garantindo que sempre haja espaço
        ///    suficiente para a nova inserção.
        /// 
        /// 5. EXECUTA A EXCLUSÃO:
        ///    Busca os IDs dos documentos mais antigos (ordenando pela 'Data' ascendente).
        ///    Executa um 'DeleteManyAsync' para remover todos de uma vez.
        /// </remarks>
        /// <param name="quantidadeNovosDocumentos">O número de documentos que estão prestes a ser inseridos.</param>
*/
            long limiteMaxBytes = config.LimiteMaximoBytes;
            double percentualAlvo = config.PercentualAlvoOcupacao;
            long tamanhoAtual = await ObterTamanhoAtualAsync();

            if (tamanhoAtual >= limiteMaxBytes)
            {
                Console.WriteLine("Limite de armazenamento atingido. Limpando registros antigos...");

                var total = await _ovniDataCollection.CountDocumentsAsync(FilterDefinition<OvniData>.Empty);

                double percentualParaRemover = Math.Max(0.1, 1.0 - percentualAlvo);

                var quantidadeParaRemover = Math.Max(quantidadeNovosDocumentos, (int)(total * percentualParaRemover));

                // Busca os IDs dos documentos mais antigos
                var antigos = await _ovniDataCollection
                    .Find(FilterDefinition<OvniData>.Empty)
                    .Sort(Builders<OvniData>.Sort.Ascending(x => x.Data))
                    .Limit(quantidadeParaRemover)
                    .Project(x => x.Id)
                    .ToListAsync();

                if (antigos.Any())
                {
                    var filtroRemocao = Builders<OvniData>.Filter.In(x => x.Id, antigos);
                    await _ovniDataCollection.DeleteManyAsync(filtroRemocao);
                    Console.WriteLine($"Removidos {antigos.Count} registros antigos para liberar espaço.");
                }
            }
            else
            {
                Console.WriteLine($" Espaço OK: {tamanhoAtual} bytes / Limite {limiteMaxBytes} bytes");
            }
        }
    }
}