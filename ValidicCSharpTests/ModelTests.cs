﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ValidicCSharp;
using ValidicCSharp.Model;
using ValidicCSharp.Request;
using ValidicCSharp.Utility;

namespace ValidicCSharpTests
{
    public class ModelTests
    {
        private const string UserUnderTest = "51c7dc676dedda04f9000011";
        private const string OrganizationUnderTest = "51aca5a06dedda916400002b";

        [Test]
        public void InitialDeserializationWorks()
        {
            var client = new Client { AccessToken = "ENTERPRISE_KEY" };
            var json = client.ExecuteWebCommand("organizations/51aca5a06dedda916400002b.json?start_date=09-01-01", HttpMethod.GET);
            var org = json.ToResult<Organization>();
            
            Assert.IsTrue(org.Summary.Limit == 100);
            Assert.IsTrue(org.Object.As<Organization>().Name == "ACME Corp");
        }

        [Test]
        public void AppModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Apps)
                .FromUser(UserUnderTest);
            var json = client.PerformCommand(command);
            var applications = json.Objectify<Apps>().AppCollection;

            Assert.IsTrue(applications.Count > 0);
        }

        [Test]
        public void ActivitiesModelPopulatesCorrectly()
        {
            var client = new Client() { AccessToken = "DEMO_KEY" };
            var command = new Command()
                .GetUser(UserUnderTest);

            var json = client.PerformCommand(command);
            var user = json.ToResult<List<Activity>>("activities");

            Assert.IsTrue(user.Object.As<List<Activity>>().Count == 100);
        }

        [Test]
        public void MyModelPopultesCorrectly()
        {
            var client = new Client{ AccessToken = "563a65efe369f63803fb022d26d549488730bb858be617ab0264d56e4ad3e2c5" };
            var command = new Command()
                .GetInformationType(CommandType.Me);

            var json = client.PerformCommand(command);
            var me = json.Objectify<Credentials>().me;

            Assert.IsTrue(me.Id == "5238a4c26deddafb51000001");
        }

        [Test]
        public void CanAddUser()
        {
            var client = new Client { AccessToken = "563a65efe369f63803fb022d26d549488730bb858be617ab0264d56e4ad3e2c5" };
            var command = new Command()
                .AddUser(new AddUserRequest { access_token = client.AccessToken, user = new UserRequest { uid = MakeRandom().ToString() } })
                .ToOrganization("5238a4616deddaaefc000001");

            var json = client.PerformCommand(command);
            var response = json.Objectify<AddUserResponse>();

            Assert.IsTrue(response.user._id != null);
            Assert.IsTrue(response.code.Equals(201));
        }

        [Test]
        public void CanAddUserWithProfile()
        {
            var client = new Client { AccessToken = "563a65efe369f63803fb022d26d549488730bb858be617ab0264d56e4ad3e2c5" };
            //make a user request object
            var newUserWithProfile = new UserRequest {uid = MakeRandom().ToString()};
            //add a profile opbject to the newly created request object
            newUserWithProfile.profile = new Profile { Country = "United States", Gender = GenderType.M, Weight = 125 };
            //create a new command to "add user" and "to organization"
            var command = new Command()
                .AddUser(new AddUserRequest { access_token = client.AccessToken, user = newUserWithProfile })
                .ToOrganization("5238a4616deddaaefc000001");
            //execute the command
            var json = client.PerformCommand(command);
            //deserialize the json into a userresponse object
            var response = json.Objectify<AddUserResponse>();

            Assert.IsTrue(response.user._id != null);
            Assert.IsTrue(response.code.Equals(201));
            Assert.IsTrue(response.user.profile.Gender == GenderType.M);
        }

        [Test]
        public void ProfileModelPopulatesCorrectly()
        {
            var client = new Client{ AccessToken = "563a65efe369f63803fb022d26d549488730bb858be617ab0264d56e4ad3e2c5" };
            var command = new Command()
                .GetInformationType(CommandType.Profile);

            var json = client.PerformCommand(command);
            var profile = json.ToResult<Profile>();

            Assert.IsTrue(profile.Object.As<Profile>().Gender == GenderType.F);

        }

        [Test]
        public void FitnessModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Fitness)
                .FromUser(UserUnderTest);

            var json = client.PerformCommand(command);
            var fitness = json.ToResult<List<Fitness>>();

            Assert.IsTrue(fitness.Object.As<List<Fitness>>().First().calories != null);
            Assert.IsTrue(fitness.Summary.Status == StatusCode.Ok);


        }

        [Test]
        public void FitnessModelPopulatesFromEnterpriseCall()
        {
            var client = new Client { AccessToken = "ENTERPRISE_KEY" };
            var command = new Command()
                .FromOrganization(OrganizationUnderTest)
                .GetInformationType(CommandType.Fitness)
                .GetUser(UserUnderTest);
            var json = client.PerformCommand(command);
            var fitness = json.ToResult<List<Fitness>>();
            Assert.IsTrue(fitness.Object.First()._id != null);
        }

        [Test]
        public void ListOfUsersFromOrganizationParsesCorrectly()
        {
            var client = new Client { AccessToken = "ENTERPRISE_KEY" };
            var command = new Command()
               .GetUsers()
               .FromOrganization(OrganizationUnderTest);
            var json = client.PerformCommand(command);

            var users = json.ToResult<List<Me>>("users");
            Assert.True(users.Object.Count > 0);
        }

        [Test]
        public void RoutineModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Routine)
                .FromUser(UserUnderTest);
               var json = client.PerformCommand(command);

               var routine = json.ToResult<List<Routine>>();
               Assert.True(routine.Object.First()._id != null);
        }

        [Test]
        public void NutritionModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Nutrition)
                .FromUser(UserUnderTest);
            var json = client.PerformCommand(command);

            var nutrition = json.ToResult<List<Nutrition>>();
            Assert.True(nutrition.Summary.Message.Equals("Ok"));
        }

        [Test]
        public void SleepModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Sleep)
                .FromUser(UserUnderTest)
                .FromDate("09-01-01");
            var json = client.PerformCommand(command);

            var sleep = json.ToResult<List<Sleep>>();
            Assert.True(sleep.Object.First()._id != null);
        }

        [Test]
        public void WeightModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Weight)
                .FromUser(UserUnderTest)
                .FromDate("09-01-01");
            var json = client.PerformCommand(command);

            var weight = json.ToResult<List<Weight>>();
            Assert.True(weight.Object.First()._id != null);
        }

        [Test]
        public void DiabetesModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Diabetes)
                .FromUser(UserUnderTest)
                .FromDate("09-01-01");
            var json = client.PerformCommand(command);

            var diabetes = json.ToResult<List<Diabetes>>();
            Assert.True(diabetes.Object.First()._id != null);
        }

        [Test]
        public void BiometricsModelPopulatesCorrectly()
        {
            var client = new Client();
            var command = new Command()
                .GetInformationType(CommandType.Biometrics)
                .FromUser(UserUnderTest)
                .FromDate("09-01-01");
            var json = client.PerformCommand(command);

            var biometrics = json.ToResult<List<Biometrics>>();
            Assert.True(biometrics.Object.First()._id != null);
        }

        [Test]
        public void TobaccoOrgModelPopulatesCorrectly()
        {
            var client = new Client { AccessToken = "ENTERPRISE_KEY" };
            var command = new Command()
                .GetInformationType(CommandType.Tobacco_Cessation)
                .FromOrganization(OrganizationUnderTest)
                .FromDate("09-01-01");
            var json = client.PerformCommand(command);

            var tobacco = json.ToResult<List<Tobacco_Cessation>>();
            Assert.True(tobacco.Object.First()._id != null);
        }

        private int MakeRandom(int to = 10000)
        {
            Random random = new Random();
            return random.Next(0, to);
        }

    }
}
