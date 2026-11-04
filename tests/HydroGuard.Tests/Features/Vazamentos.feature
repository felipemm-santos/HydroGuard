# language: pt
Funcionalidade: Detecção de possível vazamento

  Cenário: Alertar quando a leitura ultrapassa a média histórica em mais de 40%
    Dado que estou autenticado e tenho um medidor cadastrado
    E que o medidor tem uma leitura histórica de 100 litros
    Quando registro uma leitura de 200 litros e consulto os alertas
    Então recebo um alerta de severidade Alta para esse medidor
