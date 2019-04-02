using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Ddd.Infrastructure;

namespace Ddd.Taxi.Domain
{
    // In real aplication it whould be the place where database is used to find driver by its Id.
    // But in this exercise it is just a mock to simulate database
    public class DriversRepository
    {
        public void FillDriverToOrder(int driverId, TaxiOrder order)
        {
            if (driverId == 15)
            {

                order.SetCar(new TaxiOrder.Car("Lada sedan", "Baklazhan", "A123BT 66"));
                order.SetDriver(new TaxiOrder.Driver(driverId, new PersonName("Drive", "Driverson"), order.car));
            }
            else
                throw new Exception("Unknown driver id " + driverId);
        }
    }

    public class TaxiApi : ITaxiApi<TaxiOrder>
    {
        private readonly DriversRepository driversRepo;
        private readonly Func<DateTime> currentTime;
        private int idCounter;

        public TaxiApi(DriversRepository driversRepo, Func<DateTime> currentTime)
        {
            this.driversRepo = driversRepo;
            this.currentTime = currentTime;
        }

        public TaxiOrder CreateOrderWithoutDestination(string firstName, string lastName, string street, string building)
        {
            return TaxiOrder.CreateOrderWithoutDestination(idCounter, driversRepo, currentTime, firstName, lastName, street, building);

        }

        public void UpdateDestination(TaxiOrder order, string street, string building)
        {
            order.UpdateDestination(street, building);
        }

        public void AssignDriver(TaxiOrder order, int driverId)
        {
            order.AssignDriver(idCounter, driversRepo, currentTime, order, driverId);
        }

        public void UnassignDriver(TaxiOrder order)
        {
            TaxiOrder.UnassignDriver(order);
        }

        public string GetDriverFullInfo(TaxiOrder order)
        {
            return order.GetDriverFullInfo();
        }

        public string GetShortOrderInfo(TaxiOrder order)
        {
            return order.GetShortOrderInfo();
        }

        private DateTime GetLastProgressTime(TaxiOrder order)
        {
            return order.GetLastProgressTime();
        }





        public void Cancel(TaxiOrder order)
        {
            order.Cancel(currentTime, order);
        }

        public void StartRide(TaxiOrder order)
        {
            order.StartRide(currentTime);
        }

        public void FinishRide(TaxiOrder order)
        {
            order.FinishRide(currentTime);
        }
    }

    public class TaxiOrder : Entity<int>
    {
        public int Id { get; private set; }
        public PersonName ClientName { get; private set; }
        //public Route route;
        public Address Start { get; private set; }
        public Address Destination { get; private set; }
        public Driver driver { get; private set; }
        public Car car { get; private set; }
        public TaxiOrderStatus Status { get; private set; }
        public TravelTime travelTime { get; private set; }

        public void SetDriver(Driver driver)
        {
            this.driver = driver;
        }
        public void SetCar(Car car)
        {
            this.car = car;
        }
        public TaxiOrder(int id) : base(id)
        {
            Id = id;
            ClientName = new PersonName(null, null);
            Start = new Address(null, null);
            Destination = new Address(null, null);
            car = new Car(null, null, null);
            driver = new Driver(0, new PersonName(null, null), car);
            Status = default(TaxiOrderStatus);
            travelTime = new TravelTime(default(DateTime), default(DateTime),
                default(DateTime), default(DateTime), default(DateTime));
        }
        public static TaxiOrder CreateOrderWithoutDestination(
            int idCounter,
            DriversRepository driversRepo,
            Func<DateTime> currentTime,
            string firstName,
            string lastName,
            string street,
            string building)
        {
            return
                new TaxiOrder(0)
                {
                    Id = idCounter++,
                    ClientName = new PersonName(firstName, lastName),
                    Start = new Address(street, building),
                    travelTime = new TravelTime(currentTime(),
                    default(DateTime), default(DateTime),
                    default(DateTime), default(DateTime))
                };
        }
        public void UpdateDestination(string street, string building)
        {
            Destination = new Address(street, building);
        }
        public void AssignDriver(int idCounter,
            DriversRepository driversRepo,
            Func<DateTime> currentTime, TaxiOrder order, int driverId)
        {
            driversRepo.FillDriverToOrder(driverId, order);
            order.travelTime.DriverAssignmentTime = currentTime();
            order.Status = TaxiOrderStatus.WaitingCarArrival;
        }
        public static void UnassignDriver(TaxiOrder order)
        {
            if (order.Status == TaxiOrderStatus.WaitingForDriver) throw new InvalidOperationException("WaitingForDriver");
            order.driver.personName = new PersonName(null, null);
            order.car.CarModel = null;
            order.car.CarColor = null;
            order.car.CarPlateNumber = null;
            order.Status = TaxiOrderStatus.WaitingForDriver;
        }
        public string GetDriverFullInfo()
        {
            if (Status == TaxiOrderStatus.WaitingForDriver) return null;
            return string.Join(" ",
                "Id: " + driver.id,
                "DriverName: " + FormatName(driver.personName.FirstName, driver.personName.LastName),
                "Color: " + car.CarColor,
                "CarModel: " + car.CarModel,
                "PlateNumber: " + car.CarPlateNumber);
        }
        public string GetShortOrderInfo()
        {
            return string.Join(" ",
                "OrderId: " + Id,
                "Status: " + Status,
                "Client: " + FormatName(ClientName.FirstName, ClientName.LastName),
                "Driver: " + FormatName(driver.personName.FirstName, driver.personName.LastName),
                "From: " + FormatAddress(Start.Street, Start.Building),
                "To: " + FormatAddress(Destination.Street, Destination.Building),
                "LastProgressTime: " + GetLastProgressTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
        public DateTime GetLastProgressTime()
        {
            if (Status == TaxiOrderStatus.WaitingForDriver) return travelTime.CreationTime;
            if (Status == TaxiOrderStatus.WaitingCarArrival) return travelTime.DriverAssignmentTime;
            if (Status == TaxiOrderStatus.InProgress) return travelTime.StartRideTime;
            if (Status == TaxiOrderStatus.Finished) return travelTime.FinishRideTime;
            if (Status == TaxiOrderStatus.Canceled) return travelTime.CancelTime;
            throw new NotSupportedException(Status.ToString());
        }
        public void Cancel(Func<DateTime> currentTime, TaxiOrder order)
        {
            order.Status = TaxiOrderStatus.Canceled;
            order.travelTime.CancelTime = currentTime();
        }
        public void StartRide(Func<DateTime> currentTime)
        {
            Status = TaxiOrderStatus.InProgress;
            travelTime.StartRideTime = currentTime();
        }
        public void FinishRide(Func<DateTime> currentTime)
        {
            Status = TaxiOrderStatus.Finished;
            travelTime.FinishRideTime = currentTime();
        }
        private string FormatName(string firstName, string lastName)
        {
            return string.Join(" ", new[] { firstName, lastName }.Where(n => n != null));
        }
        private string FormatAddress(string street, string building)
        {
            return string.Join(" ", new[] { street, building }.Where(n => n != null));
        }
        public class Driver : Entity<int>
        {
            public int id { get; private set; }
            public PersonName personName;
            public Car car { get; private set; }
            public string CarColor { get; private set; }
            public string CarModel { get; private set; }
            public string CarPlateNumber { get; private set; }
            public Driver(int id, PersonName personName, Car car) : base(id)
            {
                this.car = car;
                this.id = id;
                this.personName = personName;
            }
        }
        public class Car : ValueType<Car>
        {
            public string CarColor;
            public string CarModel;
            public string CarPlateNumber;
            public Car(string CarModel, string CarColor, string CarPlateNumber)
            {
                this.CarColor = CarColor;
                this.CarModel = CarModel;
                this.CarPlateNumber = CarPlateNumber;
            }
        }
        public class TravelTime
        {
            public DateTime CreationTime;
            public DateTime DriverAssignmentTime;
            public DateTime CancelTime;
            public DateTime StartRideTime;
            public DateTime FinishRideTime;
            public TravelTime(
                DateTime CreationTime,
                DateTime DriverAssignmentTime,
                DateTime CancelTime,
                DateTime StartRideTime,
                DateTime FinishRideTime)
            {
                this.CreationTime = CreationTime;
                this.DriverAssignmentTime = DriverAssignmentTime;
                this.CancelTime = CancelTime;
                this.StartRideTime = StartRideTime;
                this.FinishRideTime = FinishRideTime;
            }
        }
    }
}