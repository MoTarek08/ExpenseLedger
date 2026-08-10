using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expense_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_categories", x => x.id);
                });

            // COMMENTED OUT: ObjectStorageDeletionRequest table is no longer used
            //migrationBuilder.CreateTable(
            //    name: "object_storage_deletion_requests",
            //    columns: table => new
            //    {
            //        id = table.Column<Guid>(type: "uuid", nullable: false),
            //        object_key = table.Column<string>(type: "text", nullable: false),
            //        storage_provider = table.Column<int>(type: "int", nullable: false),
            //        created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_object_storage_deletion_requests", x => x.id);
            //    });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", maxLength: 100, nullable: false),
                    display_name = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    role = table.Column<int>(type: "int", nullable: false),
                    registered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    email_verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expense_sub_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "text", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_sub_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_expense_sub_categories_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.CheckConstraint("CK_refresh_tokens_timestamps", "expires_at >= created_at");
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spending_goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", maxLength: 500, nullable: true),
                    maximum_target_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    minimum_target_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    starts_at = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_at = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spending_goals", x => x.id);
                    table.CheckConstraint("CK_spending_goals_bounds", "minimum_target_amount > 0 AND maximum_target_amount >= minimum_target_amount");
                    table.CheckConstraint("CK_spending_goals_ends_at", "ends_at >= starts_at");
                    table.ForeignKey(
                        name: "FK_spending_goals_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_spending_goals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_category_preferences",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preference_level = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_category_preferences", x => new { x.user_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_user_category_preferences_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_category_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_financial_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_net_income = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reset_day = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_financial_profiles", x => x.id);
                    table.CheckConstraint("CK_users_financial_profiles_monthly_net_income", "monthly_net_income >= 0");
                    table.ForeignKey(
                        name: "FK_users_financial_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scheduled_expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cadence = table.Column<int>(type: "int", nullable: false),
                    first_due_on = table.Column<DateOnly>(type: "date", nullable: false),
                    next_due_on = table.Column<DateOnly>(type: "date", nullable: true),
                    last_processed_at = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scheduled_expenses", x => x.id);
                    table.CheckConstraint("CK_scheduled_expenses_active_next_due_on", "is_active = TRUE OR next_due_on IS NULL");
                    table.CheckConstraint("CK_scheduled_expenses_amount", "amount > 0");
                    table.CheckConstraint("CK_scheduled_expenses_last_processed_at", "last_processed_at IS NULL OR last_processed_at >= first_due_on");
                    table.CheckConstraint("CK_scheduled_expenses_next_due_on", "next_due_on IS NULL OR next_due_on >= first_due_on");
                    table.ForeignKey(
                        name: "FK_scheduled_expenses_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scheduled_expenses_expense_sub_categories_sub_category_id",
                        column: x => x.sub_category_id,
                        principalTable: "expense_sub_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_scheduled_expenses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_generation_date = table.Column<DateOnly>(type: "date", nullable: true),
                    title = table.Column<string>(type: "text", maxLength: 100, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    spent_on = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.CheckConstraint("CK_expenses_amount", "amount > 0");
                    table.CheckConstraint("CK_expenses_scheduled_generation_date_required", "scheduled_expense_id IS NULL OR scheduled_generation_date IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_expenses_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenses_expense_sub_categories_sub_category_id",
                        column: x => x.sub_category_id,
                        principalTable: "expense_sub_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expenses_scheduled_expenses_scheduled_expense_id",
                        column: x => x.scheduled_expense_id,
                        principalTable: "scheduled_expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_expenses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses_file_objects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    object_key = table.Column<string>(type: "text", nullable: false),
                    storage_provider = table.Column<int>(type: "int", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    file_size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    started_processing_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    upload_url_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses_file_objects", x => x.id);
                    table.ForeignKey(
                        name: "FK_expenses_file_objects_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_expenses_file_objects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.CheckConstraint("CK_notes_content_length", "length(content) >= 1 AND length(content) <= 2000");
                    table.ForeignKey(
                        name: "FK_notes_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dedup_key = table.Column<string>(type: "text", maxLength: 256, nullable: false),
                    reason = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    spending_goal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    budget_period_start = table.Column<DateOnly>(type: "date", nullable: true),
                    title = table.Column<string>(type: "text", maxLength: 250, nullable: false),
                    body = table.Column<string>(type: "text", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_expense_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notifications_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notifications_scheduled_expenses_scheduled_expense_id",
                        column: x => x.scheduled_expense_id,
                        principalTable: "scheduled_expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notifications_spending_goals_spending_goal_id",
                        column: x => x.spending_goal_id,
                        principalTable: "spending_goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_expense_categories_code",
                table: "expense_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expense_sub_categories_category_id",
                table: "expense_sub_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "UQ_expense_sub_categories_code",
                table: "expense_sub_categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_category_id",
                table: "expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_sub_category_id",
                table: "expenses",
                column: "sub_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_user_id_spent_on",
                table: "expenses",
                columns: new[] { "user_id", "spent_on" });

            migrationBuilder.CreateIndex(
                name: "UQ_expenses_scheduled_expense_id_scheduled_generation_date",
                table: "expenses",
                columns: new[] { "scheduled_expense_id", "scheduled_generation_date" },
                unique: true,
                filter: "scheduled_expense_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_file_objects_expense_id",
                table: "expenses_file_objects",
                column: "expense_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_file_objects_status",
                table: "expenses_file_objects",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_file_objects_user_id",
                table: "expenses_file_objects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_expenses_file_objects_object_key",
                table: "expenses_file_objects",
                column: "object_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notes_expenses_expense_id",
                table: "notes",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_category_id",
                table: "notifications",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_expense_id",
                table: "notifications",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_scheduled_expense_id",
                table: "notifications",
                column: "scheduled_expense_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_spending_goal_id",
                table: "notifications",
                column: "spending_goal_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_deleted_at_created_at",
                table: "notifications",
                columns: new[] { "user_id", "deleted_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_read_at",
                table: "notifications",
                columns: new[] { "user_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "UQ_notifications_user_id_deduplication_key",
                table: "notifications",
                columns: new[] { "user_id", "dedup_key" },
                unique: true);

            // COMMENTED OUT: ObjectStorageDeletionRequest table is no longer used
            //migrationBuilder.CreateIndex(
            //    name: "IX_object_storage_deletion_requests_processed_at",
            //    table: "object_storage_deletion_requests",
            //    column: "processed_at");

            //migrationBuilder.CreateIndex(
            //    name: "UQ_object_storage_deletion_requests_object_key",
            //    table: "object_storage_deletion_requests",
            //    column: "object_key",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_revoked_at",
                table: "refresh_tokens",
                column: "revoked_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_active_refresh_token_session_id",
                table: "refresh_tokens",
                column: "session_id",
                unique: true,
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_expenses_cadence",
                table: "scheduled_expenses",
                column: "cadence");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_expenses_category_id",
                table: "scheduled_expenses",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_expenses_sub_category_id",
                table: "scheduled_expenses",
                column: "sub_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_scheduled_expenses_user_id",
                table: "scheduled_expenses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_spending_goals_category_id",
                table: "spending_goals",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_spending_goals_user_id",
                table: "spending_goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_spending_goals_user_category_period",
                table: "spending_goals",
                columns: new[] { "user_id", "category_id", "starts_at", "ends_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_category_preferences_category_id",
                table: "user_category_preferences",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role",
                table: "users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "UQ_user_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_financial_profiles_reset_day",
                table: "users_financial_profiles",
                column: "reset_day");

            migrationBuilder.CreateIndex(
                name: "IX_users_financial_profiles_user_id",
                table: "users_financial_profiles",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses_file_objects");

            migrationBuilder.DropTable(
                name: "notes");

            migrationBuilder.DropTable(
                name: "notifications");

            // COMMENTED OUT: ObjectStorageDeletionRequest table is no longer used
            //migrationBuilder.DropTable(
            //    name: "object_storage_deletion_requests");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_category_preferences");

            migrationBuilder.DropTable(
                name: "users_financial_profiles");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "spending_goals");

            migrationBuilder.DropTable(
                name: "scheduled_expenses");

            migrationBuilder.DropTable(
                name: "expense_sub_categories");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "expense_categories");
        }
    }
}
