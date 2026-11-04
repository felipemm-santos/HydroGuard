# language: pt
Funcionalidade: Registro de consumo de água

  Cenário: Registrar uma leitura válida
    Dado que estou autenticado e tenho um medidor cadastrado
    Quando registro uma leitura de 250 litros
    Então a leitura é criada com os dados enviados e aparece no histórico

  Cenário: Rejeitar uma leitura negativa
    Dado que estou autenticado e tenho um medidor cadastrado
    Quando registro uma leitura de -10 litros
    Então a API rejeita a leitura e o histórico permanece vazio
