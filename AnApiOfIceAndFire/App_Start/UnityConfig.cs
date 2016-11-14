using System;
using System.Data.Entity;
using AnApiOfIceAndFire.Data;
using AnApiOfIceAndFire.Data.Entities;
using AnApiOfIceAndFire.Domain;
using AnApiOfIceAndFire.Domain.Books;
using AnApiOfIceAndFire.Domain.Characters;
using AnApiOfIceAndFire.Domain.Houses;
using AnApiOfIceAndFire.Infrastructure;
using AnApiOfIceAndFire.Infrastructure.Links;
using AnApiOfIceAndFire.Models.v1;
using AnApiOfIceAndFire.Models.v1.Mappers;
using Geymsla;
using Geymsla.EntityFramework;
using Microsoft.Practices.Unity;
using Gender = AnApiOfIceAndFire.Domain.Characters.Gender;
using MediaType = AnApiOfIceAndFire.Domain.Books.MediaType;

namespace AnApiOfIceAndFire
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static readonly Lazy<IUnityContainer> Container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return Container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();    

            container.RegisterType<ISecondLevelCacheSettings, DefaultSecondLevelCacheSettings>(new InjectionConstructor(true, TimeSpan.FromHours(12)));
            container.RegisterType<DbContext, AnApiOfIceAndFireContext>();
            container.RegisterType<IReadOnlyRepository<BookEntity, int>, EntityFrameworkRepository<BookEntity, int>>();
            container.RegisterType<IReadOnlyRepository<CharacterEntity, int>, EntityFrameworkRepository<CharacterEntity, int>>();
            container.RegisterType<IReadOnlyRepository<HouseEntity, int>, EntityFrameworkRepository<HouseEntity, int>>();
            container.RegisterType<IModelMapper<Gender, Models.v1.Gender>, GenderMapper>();
            container.RegisterType<IModelMapper<MediaType, Models.v1.MediaType>, MediaTypeMapper>();
            container.RegisterType<IModelMapper<IBook, Book>, BookMapper>();
            container.RegisterType<IModelMapper<ICharacter, Character>, CharacterMapper>();
            container.RegisterType<IModelMapper<IHouse, House>, HouseMapper>();
            container.RegisterType<IModelService<IBook,BookFilter>, BookService>();
            container.RegisterType<IModelService<ICharacter, CharacterFilter>, CharacterService>();
            container.RegisterType<IModelService<IHouse, HouseFilter>, HouseService>();
            container.RegisterType<IPagingLinksFactory<BookFilter>, BookPagingLinksFactory>();
            container.RegisterType<IPagingLinksFactory<CharacterFilter>, CharacterPagingLinksFactory>();
            container.RegisterType<IPagingLinksFactory<HouseFilter>, HousePagingLinksFactory>();
            container.RegisterType<IApiSettings, WebConfigSettings>();
        }
    }
}