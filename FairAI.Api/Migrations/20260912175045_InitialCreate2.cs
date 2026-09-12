using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FairAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiStateRecords_ChatSessions_SessionId",
                table: "AiStateRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_CoreRecords_ChatSessions_SessionId",
                table: "CoreRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_NeuronRecords_ChatSessions_SessionId",
                table: "NeuronRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeRecords_ChatSessions_SessionId",
                table: "NodeRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NodeRecords",
                table: "NodeRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NeuronSet",
                table: "NeuronSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NeuronRecords",
                table: "NeuronRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LanguageEntries",
                table: "LanguageEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataSet",
                table: "DataSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoreSet",
                table: "CoreSet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoreRecords",
                table: "CoreRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AiStateRecords",
                table: "AiStateRecords");

            migrationBuilder.RenameTable(
                name: "NodeRecords",
                newName: "noderecords");

            migrationBuilder.RenameTable(
                name: "NeuronSet",
                newName: "neuronset");

            migrationBuilder.RenameTable(
                name: "NeuronRecords",
                newName: "neuronrecords");

            migrationBuilder.RenameTable(
                name: "LanguageEntries",
                newName: "languageentries");

            migrationBuilder.RenameTable(
                name: "DataSet",
                newName: "dataset");

            migrationBuilder.RenameTable(
                name: "CoreSet",
                newName: "coreset");

            migrationBuilder.RenameTable(
                name: "CoreRecords",
                newName: "corerecords");

            migrationBuilder.RenameTable(
                name: "ChatSessions",
                newName: "chatsessions");

            migrationBuilder.RenameTable(
                name: "ChatMessages",
                newName: "chatmessages");

            migrationBuilder.RenameTable(
                name: "AiStateRecords",
                newName: "aistaterecords");

            migrationBuilder.RenameIndex(
                name: "IX_NodeRecords_SessionId",
                table: "noderecords",
                newName: "IX_noderecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_NeuronRecords_SessionId",
                table: "neuronrecords",
                newName: "IX_neuronrecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_LanguageEntries_Word",
                table: "languageentries",
                newName: "IX_languageentries_Word");

            migrationBuilder.RenameIndex(
                name: "IX_CoreRecords_SessionId",
                table: "corerecords",
                newName: "IX_corerecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_SessionId",
                table: "chatmessages",
                newName: "IX_chatmessages_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_AiStateRecords_SessionId",
                table: "aistaterecords",
                newName: "IX_aistaterecords_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_noderecords",
                table: "noderecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_neuronset",
                table: "neuronset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_neuronrecords",
                table: "neuronrecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_languageentries",
                table: "languageentries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dataset",
                table: "dataset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_coreset",
                table: "coreset",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_corerecords",
                table: "corerecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chatsessions",
                table: "chatsessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chatmessages",
                table: "chatmessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_aistaterecords",
                table: "aistaterecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_aistaterecords_chatsessions_SessionId",
                table: "aistaterecords",
                column: "SessionId",
                principalTable: "chatsessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chatmessages_chatsessions_SessionId",
                table: "chatmessages",
                column: "SessionId",
                principalTable: "chatsessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_corerecords_chatsessions_SessionId",
                table: "corerecords",
                column: "SessionId",
                principalTable: "chatsessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_neuronrecords_chatsessions_SessionId",
                table: "neuronrecords",
                column: "SessionId",
                principalTable: "chatsessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_noderecords_chatsessions_SessionId",
                table: "noderecords",
                column: "SessionId",
                principalTable: "chatsessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aistaterecords_chatsessions_SessionId",
                table: "aistaterecords");

            migrationBuilder.DropForeignKey(
                name: "FK_chatmessages_chatsessions_SessionId",
                table: "chatmessages");

            migrationBuilder.DropForeignKey(
                name: "FK_corerecords_chatsessions_SessionId",
                table: "corerecords");

            migrationBuilder.DropForeignKey(
                name: "FK_neuronrecords_chatsessions_SessionId",
                table: "neuronrecords");

            migrationBuilder.DropForeignKey(
                name: "FK_noderecords_chatsessions_SessionId",
                table: "noderecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_noderecords",
                table: "noderecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_neuronset",
                table: "neuronset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_neuronrecords",
                table: "neuronrecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_languageentries",
                table: "languageentries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dataset",
                table: "dataset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_coreset",
                table: "coreset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_corerecords",
                table: "corerecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chatsessions",
                table: "chatsessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chatmessages",
                table: "chatmessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_aistaterecords",
                table: "aistaterecords");

            migrationBuilder.RenameTable(
                name: "noderecords",
                newName: "NodeRecords");

            migrationBuilder.RenameTable(
                name: "neuronset",
                newName: "NeuronSet");

            migrationBuilder.RenameTable(
                name: "neuronrecords",
                newName: "NeuronRecords");

            migrationBuilder.RenameTable(
                name: "languageentries",
                newName: "LanguageEntries");

            migrationBuilder.RenameTable(
                name: "dataset",
                newName: "DataSet");

            migrationBuilder.RenameTable(
                name: "coreset",
                newName: "CoreSet");

            migrationBuilder.RenameTable(
                name: "corerecords",
                newName: "CoreRecords");

            migrationBuilder.RenameTable(
                name: "chatsessions",
                newName: "ChatSessions");

            migrationBuilder.RenameTable(
                name: "chatmessages",
                newName: "ChatMessages");

            migrationBuilder.RenameTable(
                name: "aistaterecords",
                newName: "AiStateRecords");

            migrationBuilder.RenameIndex(
                name: "IX_noderecords_SessionId",
                table: "NodeRecords",
                newName: "IX_NodeRecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_neuronrecords_SessionId",
                table: "NeuronRecords",
                newName: "IX_NeuronRecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_languageentries_Word",
                table: "LanguageEntries",
                newName: "IX_LanguageEntries_Word");

            migrationBuilder.RenameIndex(
                name: "IX_corerecords_SessionId",
                table: "CoreRecords",
                newName: "IX_CoreRecords_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_chatmessages_SessionId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_aistaterecords_SessionId",
                table: "AiStateRecords",
                newName: "IX_AiStateRecords_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NodeRecords",
                table: "NodeRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NeuronSet",
                table: "NeuronSet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NeuronRecords",
                table: "NeuronRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LanguageEntries",
                table: "LanguageEntries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataSet",
                table: "DataSet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoreSet",
                table: "CoreSet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoreRecords",
                table: "CoreRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatSessions",
                table: "ChatSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AiStateRecords",
                table: "AiStateRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AiStateRecords_ChatSessions_SessionId",
                table: "AiStateRecords",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_ChatSessions_SessionId",
                table: "ChatMessages",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CoreRecords_ChatSessions_SessionId",
                table: "CoreRecords",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NeuronRecords_ChatSessions_SessionId",
                table: "NeuronRecords",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NodeRecords_ChatSessions_SessionId",
                table: "NodeRecords",
                column: "SessionId",
                principalTable: "ChatSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
