﻿using EnergyEvents.Models;

namespace EnergyEvents.Services
{
    /// <summary>
    /// Serviço responsável por exibir o menu principal da aplicação
    /// </summary>
    public class MenuService
    {
        private readonly MonitorDeEnergia _monitor;
        private readonly Logger _logger;
        private readonly AlertaService _alerta;
        private readonly Relatorio _relatorio;
        private readonly List<Dispositivo> _equipamentos;
        private readonly List<string> _falhas;

        /// <summary>
        /// Inicializa os serviços necessários para a interação com o usuário.
        /// </summary>
        public MenuService(
            MonitorDeEnergia monitor,
            Logger logger,
            AlertaService alerta,
            Relatorio relatorio,
            List<Dispositivo> equipamentos,
            List<string> falhas)
        {
            _monitor = monitor;
            _logger = logger;
            _alerta = alerta;
            _relatorio = relatorio;
            _equipamentos = equipamentos;
            _falhas = falhas;
        }

        public void ExibirMenu()
        {
            bool executando = true;

            while (executando)
            {
                Console.WriteLine("\n--- MENU PRINCIPAL ---");
                Console.WriteLine("1. Editar Tensão Atual");
                Console.WriteLine("2. Exibir Status dos Dispositivos");
                Console.WriteLine("3. Exibir Saídas do Sistema");
                Console.WriteLine("4. Porcentagem de Equipamentos Ligados");
                Console.WriteLine("5. Verificar Voltagem Atual");
                Console.WriteLine("6. Sair");
                Console.Write("Escolha uma opção: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        EditarTensao();
                        break;
                    case "2":
                        _relatorio.ExibirStatus();
                        break;
                    case "3":
                        ExibirSaidas();
                        break;
                    case "4":
                        MostrarPercentualLigados();
                        break;
                    case "5":
                        Console.WriteLine($"\nVoltagem atual: {_monitor.ObterUltimaTensao()}V");
                        break;
                    case "6":
                        executando = false;
                        Console.WriteLine("Encerrando aplicação...");
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
        }

        /// <summary>
        /// Solicita ao usuário que simule a mudança da tensão
        /// processa os efeitos sobre os dispositivos e registra os eventos.
        /// </summary>
        private void EditarTensao()
        {
            Console.Write("\nDigite a tensão atual (ex: 110): ");
            string input = Console.ReadLine();

            try
            {
                double tensao = double.Parse(input);
                if (tensao <= 0)
                    throw new ArgumentException("A tensão deve ser maior que zero.");

                _logger.RegistrarEvento($"Tensão registrada: {tensao}V");
                string resultado = _monitor.VerificarTensao(tensao);

                if (resultado.StartsWith("⚠"))
                    _alerta.EnviarAlerta(resultado);
                else
                    Console.WriteLine(resultado);

                _logger.RegistrarEvento(resultado);
                _falhas.Add($"[{DateTime.Now}] {resultado}");
            }
            catch (FormatException)
            {
                Console.WriteLine("Erro: entrada inválida! Digite um número decimal.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        /// <summary>
        /// Exibe todas as saídas registradas até o momento.
        /// </summary>
        private void ExibirSaidas()
        {
            Console.WriteLine("\nSaídas registradas:");
            if (_falhas.Count == 0)
                Console.WriteLine("Nenhuma saída registrada.");
            else
                _falhas.ForEach(Console.WriteLine);
        }

        /// <summary>
        /// Exibe a porcentagem de quantos aparelhos estão ligados no momento.
        /// </summary>
        private void MostrarPercentualLigados()
        {
            int total = _equipamentos.Count;
            int ligados = _equipamentos.Count(e => e.Ligado);
            double percentual = ((double)ligados / total) * 100;
            Console.WriteLine($"\nEquipamentos ligados: {ligados}/{total} ({percentual:F1}%)");
        }
    }
}
