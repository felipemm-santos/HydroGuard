using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroGuard.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration de seed - dados são inseridos via DbSeeder no Program.cs
            // Esta migration marca o ponto de seed no histórico de migrations
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback do seed não remove dados (gerenciado pelo DbSeeder)
        }
    }
}
