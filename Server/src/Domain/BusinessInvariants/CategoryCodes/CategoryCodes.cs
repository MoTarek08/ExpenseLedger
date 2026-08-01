namespace Domain.BusinessInvariants.CategoryCodesNamespace
{
    public static class CategoryCodes
    {
        public static class Food
        {
            public const string Code = "FOOD";

            public static class Groceries { public const string Code = "FOOD_GROCERIES"; }
            public static class Restaurants { public const string Code = "FOOD_RESTAURANTS"; }
            public static class FastFood { public const string Code = "FOOD_FAST_FOOD"; }
            public static class CafesCoffee { public const string Code = "FOOD_CAFES_COFFEE"; }
            public static class FoodDelivery { public const string Code = "FOOD_DELIVERY"; }
            public static class Snacks { public const string Code = "FOOD_SNACKS"; }
            public static class Other { public const string Code = "FOOD_OTHER"; }
        }

        public static class Housing
        {
            public const string Code = "HOUSING";

            public static class Rent { public const string Code = "HOUSING_RENT"; }
            public static class Mortgage { public const string Code = "HOUSING_MORTGAGE"; }
            public static class HomeMaintenance { public const string Code = "HOUSING_HOME_MAINTENANCE"; }
            public static class Furniture { public const string Code = "HOUSING_FURNITURE"; }
            public static class Appliances { public const string Code = "HOUSING_APPLIANCES"; }
            public static class HouseholdSupplies { public const string Code = "HOUSING_HOUSEHOLD_SUPPLIES"; }
            public static class Other { public const string Code = "HOUSING_OTHER"; }
        }

        public static class Transportation
        {
            public const string Code = "TRANSPORTATION";

            public static class PublicTransport { public const string Code = "TRANSPORTATION_PUBLIC_TRANSPORT"; }
            public static class TaxiRideshare { public const string Code = "TRANSPORTATION_TAXI_RIDESHARE"; }
            public static class Tolls { public const string Code = "TRANSPORTATION_TOLLS"; }
            public static class Other { public const string Code = "TRANSPORTATION_OTHER"; }
        }

        public static class VehicleOwnership
        {
            public const string Code = "VEHICLE_OWNERSHIP";

            public static class Fuel { public const string Code = "VEHICLE_OWNERSHIP_FUEL"; }
            public static class Maintenance { public const string Code = "VEHICLE_OWNERSHIP_MAINTENANCE"; }
            public static class Insurance { public const string Code = "VEHICLE_OWNERSHIP_INSURANCE"; }
            public static class Registration { public const string Code = "VEHICLE_OWNERSHIP_REGISTRATION"; }
            public static class Repairs { public const string Code = "VEHICLE_OWNERSHIP_REPAIRS"; }
            public static class CarWash { public const string Code = "VEHICLE_OWNERSHIP_CAR_WASH"; }
            public static class Other { public const string Code = "VEHICLE_OWNERSHIP_OTHER"; }
        }

        public static class Healthcare
        {
            public const string Code = "HEALTHCARE";

            public static class Doctor { public const string Code = "HEALTHCARE_DOCTOR"; }
            public static class Dentist { public const string Code = "HEALTHCARE_DENTIST"; }
            public static class Pharmacy { public const string Code = "HEALTHCARE_PHARMACY"; }
            public static class MedicalTests { public const string Code = "HEALTHCARE_MEDICAL_TESTS"; }
            public static class Therapy { public const string Code = "HEALTHCARE_THERAPY"; }
            public static class Insurance { public const string Code = "HEALTHCARE_INSURANCE"; }
            public static class Other { public const string Code = "HEALTHCARE_OTHER"; }
        }

        public static class Shopping
        {
            public const string Code = "SHOPPING";

            public static class Clothing { public const string Code = "SHOPPING_CLOTHING"; }
            public static class Shoes { public const string Code = "SHOPPING_SHOES"; }
            public static class Electronics { public const string Code = "SHOPPING_ELECTRONICS"; }
            public static class Accessories { public const string Code = "SHOPPING_ACCESSORIES"; }
            public static class Gifts { public const string Code = "SHOPPING_GIFTS"; }
            public static class Other { public const string Code = "SHOPPING_OTHER"; }
        }

        public static class Entertainment
        {
            public const string Code = "ENTERTAINMENT";

            public static class Cinema { public const string Code = "ENTERTAINMENT_CINEMA"; }
            public static class Gaming { public const string Code = "ENTERTAINMENT_GAMING"; }
            public static class Parties { public const string Code = "ENTERTAINMENT_PARTIES"; }
            public static class Outing { public const string Code = "ENTERTAINMENT_OUTING"; }
            public static class Events { public const string Code = "ENTERTAINMENT_EVENTS"; }
            public static class Hobbies { public const string Code = "ENTERTAINMENT_HOBBIES"; }
            public static class Other { public const string Code = "ENTERTAINMENT_OTHER"; }
        }

        public static class Education
        {
            public const string Code = "EDUCATION";

            public static class Courses { public const string Code = "EDUCATION_COURSES"; }
            public static class Books { public const string Code = "EDUCATION_BOOKS"; }
            public static class Tuition { public const string Code = "EDUCATION_TUITION"; }
            public static class Certifications { public const string Code = "EDUCATION_CERTIFICATIONS"; }
            public static class SchoolSupplies { public const string Code = "EDUCATION_SCHOOL_SUPPLIES"; }
            public static class Other { public const string Code = "EDUCATION_OTHER"; }
        }

        public static class Bills
        {
            public const string Code = "BILLS";

            public static class Utilities { public const string Code = "BILLS_SUBSCRIPTIONS_UTILITIES"; }
            public static class Internet { public const string Code = "BILLS_SUBSCRIPTIONS_INTERNET"; }
            public static class Phone { public const string Code = "BILLS_SUBSCRIPTIONS_PHONE"; }
            public static class Software { public const string Code = "BILLS_SUBSCRIPTIONS_SOFTWARE"; }
            public static class DigitalServices { public const string Code = "BILLS_SUBSCRIPTIONS_SERVICES"; }
            public static class Other { public const string Code = "BILLS_SUBSCRIPTIONS_OTHER"; }
        }

        public static class Financial
        {
            public const string Code = "FINANCIAL";

            public static class LoanPayment { public const string Code = "FINANCIAL_LOAN_PAYMENT"; }
            public static class CreditCardPayment { public const string Code = "FINANCIAL_CREDIT_CARD_PAYMENT"; }
            public static class BankFees { public const string Code = "FINANCIAL_BANK_FEES"; }
            public static class SavingsTransfer { public const string Code = "FINANCIAL_SAVINGS_TRANSFER"; }
            public static class Other { public const string Code = "FINANCIAL_OTHER"; }
        }

        public static class Family
        {
            public const string Code = "FAMILY";

            public static class Childcare { public const string Code = "FAMILY_CHILDCARE"; }
            public static class BabySupplies { public const string Code = "FAMILY_BABY_SUPPLIES"; }
            public static class Parents { public const string Code = "FAMILY_PARENTS"; }
            public static class Allowance { public const string Code = "FAMILY_ALLOWANCE"; }
            public static class Pets { public const string Code = "FAMILY_PETS"; }
            public static class Other { public const string Code = "FAMILY_OTHER"; }
        }

        public static class Travel
        {
            public const string Code = "TRAVEL";

            public static class Flights { public const string Code = "TRAVEL_FLIGHTS"; }
            public static class Hotels { public const string Code = "TRAVEL_HOTELS"; }
            public static class Visa { public const string Code = "TRAVEL_VISA"; }
            public static class Luggage { public const string Code = "TRAVEL_LUGGAGE"; }
            public static class Activities { public const string Code = "TRAVEL_ACTIVITIES"; }
            public static class TravelMeals { public const string Code = "TRAVEL_TRAVEL_MEALS"; }
            public static class Other { public const string Code = "TRAVEL_OTHER"; }
        }

        public static class Work
        {
            public const string Code = "WORK";

            public static class Software { public const string Code = "WORK_SOFTWARE"; }
            public static class Tools { public const string Code = "WORK_TOOLS"; }
            public static class Equipment { public const string Code = "WORK_EQUIPMENT"; }
            public static class BusinessTravel { public const string Code = "WORK_BUSINESS_TRAVEL"; }
            public static class Training { public const string Code = "WORK_TRAINING"; }
            public static class Services { public const string Code = "WORK_SERVICES"; }
            public static class Other { public const string Code = "WORK_OTHER"; }
        }

        public static class Charity
        {
            public const string Code = "CHARITY";

            public static class Donations { public const string Code = "CHARITY_DONATIONS"; }
            public static class CommunitySupport { public const string Code = "CHARITY_COMMUNITY_SUPPORT"; }
            public static class DisasterRelief { public const string Code = "CHARITY_DISASTER_RELIEF"; }
            public static class Other { public const string Code = "CHARITY_OTHER"; }
        }

        public static class Gifts
        {
            public const string Code = "GIFTS";
        }

        public static class ReligiousObligations
        {
            public const string Code = "RELIGIOUS_OBLIGATIONS";
        }

        public static class Fitness
        {
            public const string Code = "FITNESS";

            public static class GymMembership { public const string Code = "FITNESS_GYM_MEMBERSHIP"; }
            public static class GymEquipment { public const string Code = "FITNESS_GYM_EQUIPMENT"; }
            public static class PersonalTrainer { public const string Code = "FITNESS_PERSONAL_TRAINER"; }
            public static class Classes { public const string Code = "FITNESS_CLASSES"; }
            public static class Supplements { public const string Code = "FITNESS_SUPPLEMENTS"; }
            public static class FitnessApparel { public const string Code = "FITNESS_APPAREL"; }
            public static class FitnessAccessories { public const string Code = "FITNESS_ACCESSORIES"; }
            public static class Other { public const string Code = "FITNESS_OTHER"; }
        }

        public static class Sports
        {
            public const string Code = "SPORTS";

            public static class ClubMembership { public const string Code = "SPORTS_CLUB_MEMBERSHIP"; }
            public static class TeamFees { public const string Code = "SPORTS_TEAM_FEES"; }
            public static class Coaching { public const string Code = "SPORTS_COACHING"; }
            public static class TournamentFees { public const string Code = "SPORTS_TOURNAMENT_FEES"; }
            public static class LeagueFees { public const string Code = "SPORTS_LEAGUE_FEES"; }
            public static class CourtFieldRental { public const string Code = "SPORTS_COURT_FIELD_RENTAL"; }
            public static class SportsEquipment { public const string Code = "SPORTS_EQUIPMENT"; }
            public static class Uniforms { public const string Code = "SPORTS_UNIFORMS"; }
            public static class SportsTravel { public const string Code = "SPORTS_TRAVEL"; }
            public static class Other { public const string Code = "SPORTS_OTHER"; }
        }

        public static class PersonalCare
        {
            public const string Code = "PERSONAL_CARE";

            public static class Haircut { public const string Code = "PERSONAL_CARE_HAIRCUT"; }
            public static class Cosmetics { public const string Code = "PERSONAL_CARE_COSMETICS"; }
            public static class Skincare { public const string Code = "PERSONAL_CARE_SKINCARE"; }
            public static class Spa { public const string Code = "PERSONAL_CARE_SPA"; }
            public static class Gym { public const string Code = "PERSONAL_CARE_GYM"; }
            public static class Supplements { public const string Code = "PERSONAL_CARE_SUPPLEMENTS"; }
            public static class Other { public const string Code = "PERSONAL_CARE_OTHER"; }
        }

        public static class Uncategorized
        {
            public const string Code = "UNCATEGORIZED";
        }
    }
}