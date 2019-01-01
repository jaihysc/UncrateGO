using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UncrateGo.Core
{
    public class PaginationManager : InteractiveBase<SocketCommandContext>
    {
        private List<string> embedField1Master = new List<string>();
        private List<string> embedField2Master = new List<string>();
        private List<string> embedField1 = new List<string>();
        private List<string> embedField2 = new List<string>();

        private List<PaginatedMessage.Page> pages = new List<PaginatedMessage.Page>();

        /// <summary>
        /// Generates a custom pagination message at specified cutoffs per page
        /// </summary>
        /// <param name="embedField1">Left side field description</param>
        /// <param name="embedField2">Right side field description</param>
        /// <param name="entriesPerPage">Entries per page</param>
        /// <param name="paginationConfig">Customise default field parameters</param>
        /// <returns>Paginated message which can be sent</returns>
        public PaginatedMessage GeneratePaginatedMessage(List<string> embedField1Input, List<string> embedField2Input, PaginationConfig paginationConfig = null, int entriesPerPage = 10)
        {
            embedField1Master = embedField1Input;
            embedField2Master = embedField2Input;

            //If not specified, use default values
            if (paginationConfig == null)
            {
                paginationConfig = new PaginationConfig();
            }         

            //Add blank inline field if user has no skins
            if (embedField1Master.Count <= 0 && embedField2Master.Count <= 0)
            {
                pages.Add(new PaginatedMessage.Page
                {
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = paginationConfig.DefaultFieldHeader,
                            Value = paginationConfig.DefaultFieldDescription
                        }
                    }
                });
            }
            else
            {
                //Generate the user pages
                GeneratePages(paginationConfig, entriesPerPage);
            }

            //Create paginated message
            var pager = new PaginatedMessage
            {
                Pages = pages,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = paginationConfig.AuthorURL,
                    Name = paginationConfig.AuthorName,
                },
                Color = Color.DarkGreen,
                Description = paginationConfig.Description,
                FooterOverride = null,
                //ImageUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                //ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Options = PaginatedAppearanceOptions.Default,
            };

            return pager;
        }

        private void CreateNewPaginatorPage(List<string> embedField1, List<string> embedField2, List<PaginatedMessage.Page> pages, PaginationConfig paginationConfig)
        {
            pages.Add(new PaginatedMessage.Page
            {
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = paginationConfig.Field1Header,
                        Value = string.Join("\n", embedField1),
                        IsInline = true
                    },

                    new EmbedFieldBuilder
                    {
                        Name = paginationConfig.Field2Header,
                        Value = string.Join("\n", embedField2),
                        IsInline = true
                    }
                }
            });
        }

        private void GeneratePages(PaginationConfig paginationConfig, int entriesPerPage)
        {
            //Generate user page         
            int userSkinsProcessedSinceLastPage = 0;
            int counter = 0;

            //Add count of 2 fields together and divide by 2, in case the fields are ever uneven
            for (int i = 0; i < (embedField1Master.Count() + embedField2Master.Count()) / 2; i++)
            {
                try
                {
                    //Create a new page and reset counter if reached 20
                    if (userSkinsProcessedSinceLastPage == entriesPerPage)
                    {
                        //Add page
                        CreateNewPaginatorPage(embedField1, embedField2, pages, paginationConfig);

                        //Counter reset
                        userSkinsProcessedSinceLastPage = 0;

                        //Reset fields
                        embedField1 = new List<string>();
                        embedField2 = new List<string>();
                    }

                    //Keep adding skins to list if it has not reached cutoff amount
                    if (userSkinsProcessedSinceLastPage != entriesPerPage)
                    {
                        //Add items from embedFieldsMaster to working embedFields
                        embedField1.Add(embedField1Master[counter]);
                        embedField2.Add(embedField2Master[counter]);

                    }

                    //Increment counters
                    userSkinsProcessedSinceLastPage++;
                    counter++;

                }
                catch (Exception)
                {
                }
            }

            //Create final page to flush all remaining contents before exiting
            CreateNewPaginatorPage(embedField1, embedField2, pages, paginationConfig);

        }
    }

    public class PaginationConfig
    {
        public string DefaultFieldHeader { get; set; }
        public string DefaultFieldDescription { get; set; }

        public string Description { get; set; }

        public string AuthorName { get; set; }
        public string AuthorURL { get; set; }

        public string Field1Header { get; set; }
        public string Field2Header { get; set; }

        public PaginationConfig()
        {
            DefaultFieldHeader = "Default default field header";
            DefaultFieldDescription = "Default default field description";

            Description = "Default description";

            AuthorName = "Default Author name";
            AuthorURL = "https://cdn.discordapp.com/embed/avatars/0.png";

            Field1Header = "Default field 1 header";
            Field2Header = "Default field 2 header";
        }
    }
}
