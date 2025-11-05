using System;
using System.IO;

namespace AutomatedTellerMachine
{
  class BankAccount
  {
    public string UserName;
    public string AccountNumber;
    public string ExpirationDate;
    public int PIN;
    public double BalanceAccount;

    // Constructor del objeto BankAccount
    public BankAccount(string userName, string accountNumber, string expirationDate, int pin, double balanceAccount)
    {
      UserName = userName;
      AccountNumber = accountNumber;
      ExpirationDate = expirationDate;
      PIN = pin;
      BalanceAccount = balanceAccount;
    }

    // Validación de dato ExpirationDate
    public bool IsExpired()
    {
      DateTime now = DateTime.Now;
      string[] fullDate = ExpirationDate.Split("/");
      int month = int.Parse(fullDate[0]);
      int year = int.Parse(fullDate[1]) + 2000;
      return (year < now.Year) || (year < now.Year && month < now.Month);
    }

    // Validación de datos de entrada con datos registrados
    public bool IsAuth(string accNum, int pin)
    {
      return accNum == AccountNumber && pin == PIN;
    }


    // Retiro de efectivo (Solo billetes)
    public bool Withdraw(int amount)
    {
      if (amount <= 0 || amount > BalanceAccount) return false;

      bool isPosible = false;

      for (int cash50 = 0; cash50 <= amount / 50; cash50++)
      {
        int rest = amount - (cash50 * 50);

        if (rest % 20 == 0)
        {
          isPosible = true;
          break;
        }
      }

      if (!isPosible)
        return false;

      BalanceAccount -= amount;
      return true;
    }

    // Ingreso de efectivo (solo billetes)
    public bool Deposit(int amount)
    {

      if (amount <= 0) return false;

      bool isPossible = false;
      for (int cash50 = 0; cash50 <= amount / 50; cash50++)
      {
        int rest = amount - (cash50 * 50);
        if (rest % 20 == 0)
        {
          isPossible = true;
          break;
        }
      }

      if (!isPossible)
        return false;

      BalanceAccount += amount;
      return true;
    }

    // Transferir fondos
    public bool Transfer(BankAccount destinationAccount, double amount)
    {
      if (amount <= 0) return false;
      BalanceAccount -= amount;
      destinationAccount.BalanceAccount += amount;
      return true;
    }
  }
  class ATM
  {
    // Creación de lista de cuentas
    private List<BankAccount> accounts = [];
    // Declaración de objeto vacio BankAccount. 
    // se determinará el usuario actual.
    BankAccount currentBankAccount;
    // Agregado de cuentas a la lista cuentas
    public ATM()
    {
      LoadAccountsFromFile("Accounts.txt");
    }

    // MÉTODOS
    // Cargar Cuentas desde archivo
    private void LoadAccountsFromFile(string filePath)
    {
      try
      {
        // Se valida la existencia de archivo en la ruta especificada
        if (!File.Exists(filePath))
        {
          Console.WriteLine("Error, no se encontró el archivo");
          Environment.Exit(0);
        }

        // Se asigna un arreglo conformado por todas la lineas del archivo
        // [linea1, linea2, linea3, ..., lineaN]
        string[] lines = File.ReadAllLines(filePath);

        // Se itera sobre cada linea
        foreach (string line in lines)
        {
          if (string.IsNullOrWhiteSpace(line)) continue;

          // Se separa la cadena por el separador indicada
          // [userName, accountNumber, expirtationDate, pin, balanceAccount]
          string[] data = line.Split(",");

          // Se valida si la longitud de la linea es mayor a 5; Numero de atributos de la clase usuario
          if (data.Length < 5)
          {
            // Se invalida la linea y se continua con la siguiente linea
            Console.WriteLine($"Linea invalida: {line}");
            continue;
          }

          // Cada dato de cada linea se asigna a la variable correspondiente
          string userName = data[0];
          string accountNumber = data[1];
          string expirationDate = data[2];
          int pin = int.Parse(data[3]);
          double balanceAccount = double.Parse(data[4]);
          // Se crea y se añade la cuenta a la lista de cuentas
          accounts.Add(new BankAccount(userName, accountNumber, expirationDate, pin, balanceAccount));
        }

        Console.WriteLine($"{accounts.Count} cuentas cargadas correctamente desde el archivo");

      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error inesperado: {ex.Message}");
        Environment.Exit(0);
      }
    }
    // Método de inicio
    public void Start()
    {
      Console.WriteLine("------ ATM ------");
      bool goOut = false;

      // while true -> "!" -> Convierte un booleano a su contraparte
      while (!goOut)
      {
        Console.WriteLine("1. Retiro de efectivo");
        Console.WriteLine("2. Déposito de efectivo");
        Console.WriteLine("3. Consulta de saldo");
        Console.WriteLine("4. Transferencia de fondos");
        Console.WriteLine("5. Salir");
        Console.Write("Opción: ");

        // Almacenamiento de la información dentro de variable
        byte option = Byte.Parse(Console.ReadLine());

        // Estructura de control switch. Se evalua el caso en caso de que este sea igual a la opción ingresada
        // En caso de no encontrarse se muestra el caso "default"
        switch (option)
        {
          case 1:
            // Retiro
            WithdrawlMoney();
            break;
          case 2:
            // Deposito
            DepositMoney();
            break;
          case 3:
            // Consulta
            BalanceInquiry();
            break;
          case 4:
            // Transferencia
            TransferMoney();
            break;
          case 5:
            // Salir
            goOut = true;
            Console.WriteLine("Gracias por usar el cajero. Vuelva pronto!");
            break;
          default:
            Console.WriteLine("Opción invalida");
            break;
        }
      }
    }
    // Autenticación de usuario
    private bool Authentication()
    {
      // Escritura y almacenamiento de información
      Console.Write("Ingresa tu número de cuenta: ");
      string inputAccountNumber = Console.ReadLine();
      Console.Write("Ingresa tu nip: ");
      int inputPIN = int.Parse(Console.ReadLine());

      // Recorrido de todas la cuentas
      foreach (var acc in accounts)
      {
        // Comparación de la información de entrada en cada una de las cuentas
        if (acc.IsAuth(inputAccountNumber, inputPIN))
        {
          // Validación de vigencia de tarjeta
          if (acc.IsExpired())
          {
            Console.WriteLine("Tu tarjeta ha caducado. Pasa a alguna ventanilla para solicitar otra.");
            return false;
          }

          // Asignación de usuario actual en caso de tarjeta vigente
          currentBankAccount = acc;
          Console.WriteLine($"Bienvenido {currentBankAccount.UserName}");
          return true;
        }
      }

      Console.WriteLine("Datos incorrectos. Intente de nuevo");
      return false;
    }
    // Retiro de efectivo
    private void WithdrawlMoney()
    {
      // Corte de ejecución si usuario no autenticado
      if (!Authentication()) return;
      Console.WriteLine($"Saldo actual: {currentBankAccount.BalanceAccount}");
      Console.WriteLine("Ingresa la cantidad de dinero a retirar (solo billetes)");
      Console.Write("Billetes validos: $20, $50, $100, $200, $500: ");
      int amount = int.Parse(Console.ReadLine());
      if (currentBankAccount.Withdraw(amount))
      {
        Console.WriteLine($"Retiro exitoso.\nSaldo actual: ${currentBankAccount.BalanceAccount}");
      }
      else
      {
        Console.WriteLine("Saldo insuficiente o monto inválido");
      }
    }
    // Deposito de efectivo
    private void DepositMoney()
    {
      Console.Write("Ingresa el número de cuenta a depositar: ");
      string inputAccountNumber = Console.ReadLine();

      // Variable accDestiny se le asigna la cuenta a depositar
      BankAccount accDestiny = accounts.Find(account => account.AccountNumber == inputAccountNumber);

      // Validar que exista la cuenta a depositar
      if (accDestiny == null)
      {
        Console.WriteLine("No se encontro la cuenta. Asegurate que el dato sea correcto");
      }

      Console.WriteLine("Ingresa la cantidad de efectivo a ingresar (solo billetes)");
      Console.Write("Billetes validos: $20, $50, $100, $200, $500 ");
      int amount = int.Parse(Console.ReadLine());

      if (accDestiny.Deposit(amount))
      {
        Console.WriteLine("Deposito exitoso");
      }
      else
      {
        Console.WriteLine("Monto invalido");
      }
    }
    // Consulta de saldo
    private void BalanceInquiry()
    {
      if (!Authentication()) return;
      Console.WriteLine($"Tu saldo actual es: ${currentBankAccount.BalanceAccount}");
      Console.WriteLine("¿Deseas imprimir tu comprobante?: ");
      Console.Write("1.- Si\n2.- No");
      byte option = byte.Parse(Console.ReadLine());
      if (option == 1)
      {
        PrintTicket();
      }
      else
      {
        return;
      }
    }
    // Transferencia de saldo
    private void TransferMoney()
    {
      if (!Authentication()) return;
      Console.Write("Ingresa la cuenta a transferir fondos: ");
      string destinationAccount = Console.ReadLine();
      BankAccount accDestiny = accounts.Find(account => account.AccountNumber == destinationAccount);
      if (accDestiny == null)
      {
        Console.WriteLine("Cuenta no encotrada");
        return;
      }

      Console.Write("Ingresa el monto a transferir: ");
      double amount = double.Parse(Console.ReadLine());

      if (currentBankAccount.Transfer(accDestiny, amount))
      {
        Console.WriteLine($"Transferencia exitosa.\nSaldo actual: {currentBankAccount.BalanceAccount}");
      }
      else
      {
        Console.WriteLine($"No se puede realizar la transferencia, verifica tu saldo");
      }

    }
    // Imprimir comprobate
    private void PrintTicket()
    {
      string ticket = "Consulta de saldo\n" +
                      $"Fecha: {DateTime.Now}\n" +
                      $"Cliente: {currentBankAccount.UserName}\n" +
                      $"Número de cuenta: {currentBankAccount.AccountNumber}\n" +
                      $"Saldo actual: {currentBankAccount.BalanceAccount}\n";
      File.WriteAllText("Ticket.txt", ticket);
      Console.WriteLine("Comprobante generado en 'Ticket.txt'");
    }
  }
  class Application
  {
    static void Main()
    {
      ATM atm = new ATM();
      atm.Start();
    }
  }
}