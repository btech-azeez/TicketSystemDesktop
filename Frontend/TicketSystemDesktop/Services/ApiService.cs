using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "http://localhost:5000/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BASE_URL);
        }

        #region Auth Methods

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("auth/login", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<LoginResponse>(responseString) 
                    ?? new LoginResponse { Success = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<List<User>> GetAdmins()
        {
            try
            {
                var response = await _httpClient.GetAsync("auth/admins");
                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<User>>>(responseString);
                return apiResponse?.Data ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        #endregion

        #region Ticket Methods

        public async Task<ApiResponse<Ticket>> CreateTicket(CreateTicketRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("tickets", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse<Ticket>>(responseString)
                    ?? new ApiResponse<Ticket> { Success = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<Ticket>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<List<Ticket>> GetUserTickets(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"tickets/user/{userId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Ticket>>>(responseString);
                return apiResponse?.Data ?? new List<Ticket>();
            }
            catch
            {
                return new List<Ticket>();
            }
        }

        public async Task<List<Ticket>> GetAllTickets()
        {
            try
            {
                var response = await _httpClient.GetAsync("tickets/all");
                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<Ticket>>>(responseString);
                return apiResponse?.Data ?? new List<Ticket>();
            }
            catch
            {
                return new List<Ticket>();
            }
        }

        public async Task<TicketDetailsResponse?> GetTicketDetails(int ticketId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"tickets/{ticketId}");
                var responseString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<TicketDetailsResponse>>(responseString);
                return apiResponse?.Data;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ApiResponse<bool>> UpdateTicket(UpdateTicketRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("tickets", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse<bool>>(responseString)
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> AddComment(AddCommentRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("tickets/comment", content);
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ApiResponse<bool>>(responseString)
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to deserialize response" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        #endregion
    }
}
