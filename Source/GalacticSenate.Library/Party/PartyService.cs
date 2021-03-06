using GalacticSenate.Data.Implementations.EntityFramework;
using GalacticSenate.Data.Interfaces;
using GalacticSenate.Data.Interfaces.Repositories;
using GalacticSenate.Domain.Exceptions;
using GalacticSenate.Library.Party.Requests;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Model = GalacticSenate.Domain.Model;

namespace GalacticSenate.Library.Party {
   public interface IPartyService {
      Task<ModelResponse<Model.Party, AddPartyRequest>> AddAsync(AddPartyRequest request);
      Task<BasicResponse<DeletePartyRequest>> DeleteAsync(DeletePartyRequest request);
      Task<ModelResponse<Model.Party, ReadPartyMultiRequest>> ReadAsync(ReadPartyMultiRequest request);
      Task<ModelResponse<Model.Party, ReadPartyRequest>> ReadAsync(ReadPartyRequest request);
   }

   public class PartyService : IPartyService {
      private readonly IUnitOfWork<DataContext> unitOfWork;
      private readonly IPartyRepository partyRepository;

      public PartyService(IUnitOfWork<DataContext> unitOfWork) {
         this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
         this.partyRepository = unitOfWork.GetPartyRepository() ?? throw new ApplicationException("Couldn't create party repository");
      }

      public async Task<ModelResponse<Model.Party, AddPartyRequest>> AddAsync(AddPartyRequest request) {
         var response = new ModelResponse<Model.Party, AddPartyRequest>(DateTime.Now, request);

         try {
            if (request is null)
               throw new ArgumentNullException(nameof(request));

            var existing = await partyRepository.GetAsync(request.Id);

            if (existing is null) {
               existing = await partyRepository.AddAsync(new Model.Party { Id = request.Id });
               unitOfWork.Save();

               response.Messages.Add($"Party with id {request.Id} added.");
            } else {
               response.Messages.Add($"Party with id {request.Id} already exists.");
            }

            response.Results.Add(existing);

            response.Status = StatusEnum.Successful;
         } catch (Exception ex) {
            response.Status = StatusEnum.Failed;
            response.Messages.Add(ex.Message);
         }

         return response.Finalize();
      }
      public async Task<ModelResponse<Model.Party, ReadPartyMultiRequest>> ReadAsync(ReadPartyMultiRequest request) {
         var response = new ModelResponse<Model.Party, ReadPartyMultiRequest>(DateTime.Now, request);

         try {
            response.Results.AddRange(partyRepository.Get(request.PageIndex, request.PageSize));

            response.Status = StatusEnum.Successful;
         } catch (Exception ex) {
            response.Messages.Add(ex.Message);
            response.Status = StatusEnum.Failed;
         }

         return await Task.Run(() => { return response.Finalize(); });
      }
      public async Task<ModelResponse<Model.Party, ReadPartyRequest>> ReadAsync(ReadPartyRequest request) {
         var response = new ModelResponse<Model.Party, ReadPartyRequest>(DateTime.Now, request);

         try {
            var party = await partyRepository.GetAsync(request.Id);

            if (party != null)
               response.Results.Add(await partyRepository.GetAsync(request.Id));

            response.Status = StatusEnum.Successful;
         } catch (Exception ex) {
            response.Messages.Add(ex.Message);
            response.Status = StatusEnum.Failed;
         }
         return response.Finalize();
      }

      public async Task<BasicResponse<DeletePartyRequest>> DeleteAsync(DeletePartyRequest request) {
         var response = new BasicResponse<DeletePartyRequest>(DateTime.Now, request);

         try {
            await partyRepository.DeleteAsync(request.Id);
            unitOfWork.Save();

            response.Status = StatusEnum.Successful;
         } catch (Exception ex) {
            response.Status = StatusEnum.Failed;
            response.Messages.Add(ex.Message);
         }

         return response.Finalize();
      }
   }
}
