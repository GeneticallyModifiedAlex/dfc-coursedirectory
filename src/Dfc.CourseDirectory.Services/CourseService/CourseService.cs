﻿
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dfc.CourseDirectory.Common;
using Dfc.CourseDirectory.Common.Interfaces;
using Dfc.CourseDirectory.Models.Interfaces.Courses;
using Dfc.CourseDirectory.Models.Models;
using Dfc.CourseDirectory.Services.Interfaces.CourseService;
using Newtonsoft.Json;
using Dfc.CourseDirectory.Models.Models.Courses;
using System.Net;
using System.Text.RegularExpressions;
using Dfc.CourseDirectory.Common.Settings;
using System.Linq;
using Dfc.CourseDirectory.Models.Enums;
using static Dfc.CourseDirectory.Services.CourseService.CourseValidationResult;

namespace Dfc.CourseDirectory.Services.CourseService
{
    public class CourseService : ICourseService
    {
        private readonly ILogger<CourseService> _logger;
        private readonly HttpClient _httpClient;
        private readonly Uri _addCourseUri;
        private readonly Uri _getYourCoursesUri;
        private readonly Uri _updateCourseUri;
        private readonly Uri _getCourseByIdUri;
        private readonly Uri _updateStatusUri;
        private readonly Uri _getCourseCountsByStatusForUKPRNUri;
        private readonly Uri _getRecentCourseChangesByUKPRNUri;
        private readonly Uri _changeCourseRunStatusesForUKPRNSelectionUri;

        private readonly int _courseForTextFieldMaxChars;
        private readonly int _entryRequirementsTextFieldMaxChars;
        private readonly int _whatWillLearnTextFieldMaxChars;
        private readonly int _howYouWillLearnTextFieldMaxChars;
        private readonly int _whatYouNeedTextFieldMaxChars;
        private readonly int _howAssessedTextFieldMaxChars;
        private readonly int _whereNextTextFieldMaxChars;
        private readonly Uri _archiveLiveCoursesUri;


        public CourseService(
            ILogger<CourseService> logger,
            HttpClient httpClient,
            IOptions<CourseServiceSettings> settings,
            IOptions<CourseForComponentSettings> courseForComponentSettings,
            IOptions<EntryRequirementsComponentSettings> entryRequirementsComponentSettings,
            IOptions<WhatWillLearnComponentSettings> whatWillLearnComponentSettings,
            IOptions<HowYouWillLearnComponentSettings> howYouWillLearnComponentSettings,
            IOptions<WhatYouNeedComponentSettings> whatYouNeedComponentSettings,
            IOptions<HowAssessedComponentSettings> howAssessedComponentSettings,
            IOptions<WhereNextComponentSettings> whereNextComponentSettings)
        {
            Throw.IfNull(logger, nameof(logger));
            Throw.IfNull(httpClient, nameof(httpClient));
            Throw.IfNull(settings, nameof(settings));
            Throw.IfNull(courseForComponentSettings, nameof(courseForComponentSettings));
            Throw.IfNull(entryRequirementsComponentSettings, nameof(entryRequirementsComponentSettings));
            Throw.IfNull(whatWillLearnComponentSettings, nameof(whatWillLearnComponentSettings));
            Throw.IfNull(howYouWillLearnComponentSettings, nameof(howYouWillLearnComponentSettings));
            Throw.IfNull(whatYouNeedComponentSettings, nameof(whatYouNeedComponentSettings));
            Throw.IfNull(howAssessedComponentSettings, nameof(howAssessedComponentSettings));
            Throw.IfNull(whereNextComponentSettings, nameof(whereNextComponentSettings));


            _logger = logger;
            _httpClient = httpClient;

            _addCourseUri = settings.Value.ToAddCourseUri();
            _getYourCoursesUri = settings.Value.ToGetYourCoursesUri();
            _updateCourseUri = settings.Value.ToUpdateCourseUri();
            _getCourseByIdUri = settings.Value.ToGetCourseByIdUri();
            _archiveLiveCoursesUri = settings.Value.ToArchiveLiveCoursesUri();
            _updateStatusUri = settings.Value.ToUpdateStatusUri();
            _getCourseCountsByStatusForUKPRNUri = settings.Value.ToGetCourseCountsByStatusForUKPRNUri();
            _getRecentCourseChangesByUKPRNUri = settings.Value.ToGetRecentCourseChangesByUKPRNUri();
            _changeCourseRunStatusesForUKPRNSelectionUri = settings.Value.ToChangeCourseRunStatusesForUKPRNSelectionUri();

            _courseForTextFieldMaxChars = courseForComponentSettings.Value.TextFieldMaxChars;
            _entryRequirementsTextFieldMaxChars = entryRequirementsComponentSettings.Value.TextFieldMaxChars;
            _whatWillLearnTextFieldMaxChars = whatWillLearnComponentSettings.Value.TextFieldMaxChars;
            _howYouWillLearnTextFieldMaxChars = howYouWillLearnComponentSettings.Value.TextFieldMaxChars;
            _whatYouNeedTextFieldMaxChars = whatYouNeedComponentSettings.Value.TextFieldMaxChars;
            _howAssessedTextFieldMaxChars = howAssessedComponentSettings.Value.TextFieldMaxChars;
            _whereNextTextFieldMaxChars = whereNextComponentSettings.Value.TextFieldMaxChars;
        }

        public SelectRegionModel GetRegions()
        {
            return new SelectRegionModel
            {
                LabelText = "Select course region",
                HintText = "For example, South West",
                AriaDescribedBy = "Select all that apply.",
                RegionItems = new RegionItemModel[]
                {
                    new RegionItemModel
                    {
                        Id = "E12000001",
                        Checked = false,
                        RegionName = "North East",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000001"] = "County Durham",
                            ["E06000002"] = "Darlington",
                            ["E06000003"] = "Gateshead",
                            ["E06000004"] = "Hartlepool",
                            ["E06000005"] = "Middlesbrough",
                            ["E06000047"] = "Newcastle upon Tyne",
                            ["E06000057"] = "North Tyneside",
                            ["E08000021"] = "Northumberland",
                            ["E08000022"] = "Redcar and Cleveland",
                            ["E08000023"] = "South Tyneside",
                            ["E08000024"] = "Stockton-on-Tees",
                            ["E08000037"] = "Sunderland"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000002",
                        Checked = false,
                        RegionName = "North West",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000006"] = "Halton",
                            ["E06000007"] = "Warrington",
                            ["E06000008"] = "Blackburn with Darwen",
                            ["E06000009"] = "Blackpool",
                            ["E06000049"] = "Cheshire East",
                            ["E06000050"] = "Cheshire West and Chester",
                            ["E08000001"] = "Bolton",
                            ["E08000002"] = "Bury",
                            ["E08000003"] = "Manchester",
                            ["E08000004"] = "Oldham",
                            ["E08000005"] = "Rochdale",
                            ["E08000006"] = "Salford",
                            ["E08000007"] = "Stockport",
                            ["E08000008"] = "Tameside",
                            ["E08000009"] = "Trafford",
                            ["E08000010"] = "Wigan",
                            ["E08000011"] = "Knowsley",
                            ["E08000012"] = "Liverpool",
                            ["E08000013"] = "St. Helens",
                            ["E08000014"] = "Sefton",
                            ["E08000015"] = "Wirral",
                            ["E10000006"] = "Cumbria",
                            ["E10000017"] = "Lancashire"

                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000003",
                        Checked = false,
                        RegionName = "Yorkshire and The Humber",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000010"] = "Kingston upon Hull, City of",
                            ["E06000011"] = "East Riding of Yorkshire",
                            ["E06000012"] = "North East Lincolnshire",
                            ["E06000013"] = "North Lincolnshire",
                            ["E06000014"] = "York",
                            ["E08000016"] = "Barnsley",
                            ["E08000017"] = "Doncaster",
                            ["E08000018"] = "Rotherham",
                            ["E08000019"] = "Sheffield",
                            ["E08000032"] = "Bradford",
                            ["E08000033"] = "Calderdale",
                            ["E08000034"] = "Kirklees",
                            ["E08000035"] = "Leeds",
                            ["E08000036"] = "Wakefield",
                            ["E10000023"] = "North Yorkshire"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000004",
                        Checked = false,
                        RegionName = "East Midlands",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000015"] = "Derby",
                            ["E10000007"] = "Derbyshire",
                            ["E06000016"] = "Leicester",
                            ["E10000018"] = "Leicestershire",
                            ["E10000019"] = "Lincolnshire",
                            ["E10000021"] = "Northamptonshire",
                            ["E06000018"] = "Nottingham",
                            ["E10000024"] = "Nottinghamshire",
                            ["E06000017"] = "Rutland"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000005",
                        Checked = false,
                        RegionName = "West Midlands",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000019"] = "Herefordshire, County of",
                            ["E06000020"] = "Telford and Wrekin",
                            ["E06000021"] = "Stoke-on-Trent",
                            ["E06000051"] = "Shropshire",
                            ["E08000025"] = "Birmingham",
                            ["E08000026"] = "Coventry",
                            ["E08000027"] = "Dudley",
                            ["E08000028"] = "Sandwell",
                            ["E08000029"] = "Solihull",
                            ["E08000030"] = "Walsall",
                            ["E08000031"] = "Wolverhampton",
                            ["E10000028"] = "Staffordshire",
                            ["E10000031"] = "Warwickshire",
                            ["E10000034"] = "Worcestershire"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000006",
                        Checked = false,
                        RegionName = "East of England",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000055"] = "Bedford",
                            ["E10000003"] = "Cambridgeshire",
                            ["E06000056"] = "Central Bedfordshire",
                            ["E10000012"] = "Essex",
                            ["E10000015"] = "Hertfordshire",
                            ["E06000032"] = "Luton",
                            ["E10000020"] = "Norfolk",
                            ["E06000031"] = "Peterborough",
                            ["E06000033"] = "Southend-on-Sea",
                            ["E10000029"] = "Suffolk",
                            ["E06000034"] = "Thurrock"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000007",
                        Checked = false,
                        RegionName = "London",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E09000001"] = "City of London",
                            ["E09000002"] = "Barking and Dagenham",
                            ["E09000003"] = "Barnet",
                            ["E09000004"] = "Bexley,",
                            ["E09000005"] = "Brent",
                            ["E09000006"] = "Bromley",
                            ["E09000007"] = "Camden",
                            ["E09000008"] = "Croydon",
                            ["E09000009"] = "Ealing",
                            ["E09000010"] = "Enfield",
                            ["E09000011"] = "Greenwich",
                            ["E09000012"] = "Hackney",
                            ["E09000013"] = "Hammersmith and Fulham",
                            ["E09000014"] = "Haringey",
                            ["E09000015"] = "Harrow",
                            ["E09000016"] = "Havering",
                            ["E09000017"] = "Hillingdon",
                            ["E09000018"] = "Hounslow",
                            ["E09000019"] = "Islington",
                            ["E09000020"] = "Kensington and Chelsea",
                            ["E09000021"] = "Kingston upon Thames",
                            ["E09000022"] = "Lambeth",
                            ["E09000023"] = "Lewisham",
                            ["E09000024"] = "Merton",
                            ["E09000025"] = "Newham",
                            ["E09000026"] = "Redbridge",
                            ["E09000027"] = "Richmond upon Thames",
                            ["E09000028"] = "Southwark",
                            ["E09000029"] = "Sutton",
                            ["E09000030"] = "Tower Hamlets",
                            ["E09000031"] = "Waltham Forest",
                            ["E09000032"] = "Wandsworth",
                            ["E09000033"] = "Westminster"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000008",
                        Checked = false,
                        RegionName = "South East",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000035"] = "Medway",
                            ["E06000036"] = "Bracknell Forest",
                            ["E06000037"] = "West Berkshire",
                            ["E06000038"] = "Reading",
                            ["E06000039"] = "Slough",
                            ["E06000040"] = "Windsor and Maidenhead",
                            ["E06000041"] = "Wokingham",
                            ["E06000042"] = "Milton Keynes",
                            ["E06000043"] = "Brighton and Hove",
                            ["E06000044"] = "Portsmouth",
                            ["E06000045"] = "Southampton",
                            ["E06000046"] = "Isle of Wight",
                            ["E10000002"] = "Buckinghamshire",
                            ["E10000011"] = "East Sussex",
                            ["E10000014"] = "Hampshire",
                            ["E10000016"] = "Kent",
                            ["E10000025"] = "Oxfordshire",
                            ["E10000030"] = "Surrey",
                            ["E10000032"] = "West Sussex"
                        }
                    },
                    new RegionItemModel
                    {
                        Id = "E12000009",
                        Checked = false,
                        RegionName = "South West",
                        SubRegionName = new Dictionary<string, string>
                        {
                            ["E06000022"] = "Bath and North East Somerset",
                            ["E06000023"] = "Bristol, City of",
                            ["E06000024"] = "North Somerset",
                            ["E06000025"] = "South Gloucestershire",
                            ["E06000026"] = "Plymouth",
                            ["E06000027"] = "Torbay",
                            ["E06000028"] = "Bournemouth",
                            ["E06000029"] = "Poole",
                            ["E06000030"] = "Swindon",
                            ["E06000052"] = "Cornwall",
                            ["E06000053"] = "Isles of Scilly",
                            ["E06000054"] = "Wiltshire",
                            ["E10000008"] = "Devon",
                            ["E10000009"] = "Dorset",
                            ["E10000013"] = "Gloucestershire",
                            ["E10000027"] = "Somerset"
                        }
                    }

                }
            };
        }

        public async Task<IResult<ICourse>> GetCourseByIdAsync(IGetCourseByIdCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get Course By Id criteria.", criteria);
                _logger.LogInformationObject("Get Course By Id URI", _getCourseByIdUri);

                var content = new StringContent(criteria.ToJson(), Encoding.UTF8, "application/json");

                var response = await _httpClient.GetAsync(new Uri(_getCourseByIdUri.AbsoluteUri + "&id=" + criteria.Id));

                _logger.LogHttpResponseMessage("Get Course By Id service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Get Course By Id service json response", json);


                    var course = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(course);
                }
                else
                {
                    return Result.Fail<ICourse>("Get Course By Id service unsuccessful http response");
                }
            }

            catch (HttpRequestException hre)
            {
                _logger.LogException("Get Course By Id service http request error", hre);
                return Result.Fail<ICourse>("Get Course By Id service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get Course By Id service unknown error.", e);

                return Result.Fail<ICourse>("Get Course By Id service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }

        }

        public async Task<IResult<ICourseSearchResult>> GetYourCoursesByUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);

                    CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok<ICourseSearchResult>(searchResult);

                } else {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourseSearchResult>> GetCoursesByLevelForUKPRNAsync(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get your courses criteria", criteria);
                _logger.LogInformationObject("Get your courses URI", _getYourCoursesUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<ICourseSearchResult>("Get your courses unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getYourCoursesUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get your courses service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get your courses service json response", json);
                    IEnumerable<IEnumerable<IEnumerable<Course>>> courses = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<IEnumerable<Course>>>>(json);
                    var searchResult = new CourseSearchResult(courses);

                    return Result.Ok<ICourseSearchResult>(searchResult);
                }
                else
                {
                    return Result.Fail<ICourseSearchResult>("Get your courses service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Get your courses service http request error", hre);
                return Result.Fail<ICourseSearchResult>("Get your courses service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Get your courses service unknown error.", e);
                return Result.Fail<ICourseSearchResult>("Get your courses service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourseStatusCountResult>>> GetCourseCountsByStatusForUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get course counts criteria", criteria);
                _logger.LogInformationObject("Get course counts URI", _getCourseCountsByStatusForUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getCourseCountsByStatusForUKPRNUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get course counts service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get course counts service json response", json);
                    IEnumerable<ICourseStatusCountResult> counts = JsonConvert.DeserializeObject<IEnumerable<CourseStatusCountResult>>(json);

                    //CourseSearchResult searchResult = new CourseSearchResult(courses);
                    return Result.Ok<IEnumerable<ICourseStatusCountResult>>(counts);

                } else {
                    return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get course counts service http request error", hre);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get course counts service unknown error.", e);
                return Result.Fail<IEnumerable<ICourseStatusCountResult>>("Get course counts service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<IEnumerable<ICourse>>> GetRecentCourseChangesByUKPRN(ICourseSearchCriteria criteria)
        {
            Throw.IfNull(criteria, nameof(criteria));
            Throw.IfLessThan(0, criteria.UKPRN.Value, nameof(criteria.UKPRN.Value));
            _logger.LogMethodEnter();

            try
            {
                _logger.LogInformationObject("Get recent course changes criteria", criteria);
                _logger.LogInformationObject("Get recent course changes URI", _getRecentCourseChangesByUKPRNUri);

                if (!criteria.UKPRN.HasValue)
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes unknown UKRLP");

                var response = await _httpClient.GetAsync(new Uri(_getRecentCourseChangesByUKPRNUri.AbsoluteUri + "&UKPRN=" + criteria.UKPRN));
                _logger.LogHttpResponseMessage("Get recent course changes service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    if (!json.StartsWith("["))
                        json = "[" + json + "]";

                    _logger.LogInformationObject("Get recent course changes service json response", json);
                    IEnumerable<ICourse> courses = JsonConvert.DeserializeObject<IEnumerable<Course>>(json);

                    return Result.Ok<IEnumerable<ICourse>>(courses);

                } else {
                    return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unsuccessful http response");
                }

            } catch (HttpRequestException hre) {
                _logger.LogException("Get recent course changes service http request error", hre);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service http request error.");

            } catch (Exception e) {
                _logger.LogException("Get recent course changes service unknown error.", e);
                return Result.Fail<IEnumerable<ICourse>>("Get recent course changes service unknown error.");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public IResult<IList<CourseValidationResult>> PendingCourseValidationMessages(IEnumerable<ICourse> courses)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(courses, nameof(courses));

            try {
                IList<CourseValidationResult> results = new List<CourseValidationResult>();

                foreach (ICourse c in courses) {
                    CourseValidationResult cvr = new CourseValidationResult() {
                        Course = c,
                        Issues = ValidateCourse(c),
                        RunValidationResults = new List<CourseRunValidationResult>()
                    };
                    foreach (ICourseRun r in c.CourseRuns)
                        cvr.RunValidationResults.Add(new CourseRunValidationResult() { Run = r, Issues = ValidateCourseRun(r, ValidationMode.BulkUploadCourse) });
                    results.Add(cvr);
                }
                return Result.Ok(results);

            } catch (Exception ex) {
                _logger.LogException("PendingCourseValidationMessages error", ex);
                return Result.Fail<IList<CourseValidationResult>>("Error compiling messages for items requiring attention on landing page");

            } finally {
                _logger.LogMethodExit();
            }
        }

        public async Task<IResult<ICourse>> AddCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course add object.", course);
                _logger.LogInformationObject("Course add URI", _addCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_addCourseUri, content);

                _logger.LogHttpResponseMessage("Course add service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course add service json response", json);


                    var courseResult = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(courseResult);
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - TooManyRequests");
                }
                else
                {
                    return Result.Fail<ICourse>("Course add service unsuccessful http response - ResponseStatusCode: " + response.StatusCode);
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course add service http request error", hre);
                return Result.Fail<ICourse>("Course add service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course add service unknown error.", e);

                return Result.Fail<ICourse>("Course add service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }


        public async Task<IResult<ICourse>> UpdateCourseAsync(ICourse course)
        {
            _logger.LogMethodEnter();
            Throw.IfNull(course, nameof(course));

            try
            {
                _logger.LogInformationObject("Course update object.", course);
                _logger.LogInformationObject("Course update URI", _updateCourseUri);

                var courseJson = JsonConvert.SerializeObject(course);

                var content = new StringContent(courseJson, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_updateCourseUri, content);

                _logger.LogHttpResponseMessage("Course update service http response", response);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationObject("Course update service json response", json);


                    var courseResult = JsonConvert.DeserializeObject<Course>(json);


                    return Result.Ok<ICourse>(courseResult);
                }
                else
                {
                    return Result.Fail<ICourse>("Course update service unsuccessful http response");
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogException("Course update service http request error", hre);
                return Result.Fail<ICourse>("Course update service http request error.");
            }
            catch (Exception e)
            {
                _logger.LogException("Course update service unknown error.", e);

                return Result.Fail<ICourse>("Course update service unknown error.");
            }
            finally
            {
                _logger.LogMethodExit();
            }
        }

        public IList<string> ValidateCourse(ICourse course)
        {
            var validationMessages = new List<string>();

            // CourseDescription
            if (string.IsNullOrEmpty(course.CourseDescription))
            {
                validationMessages.Add("Course For decription is required");
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(course.CourseDescription))
                    validationMessages.Add("Course For decription contains invalid character");
                if (course.CourseDescription.Length > _courseForTextFieldMaxChars)
                    validationMessages.Add($"Course For decription must be { _courseForTextFieldMaxChars } characters or less");
            }

            // EntryRequirements
            if (!string.IsNullOrEmpty(course.EntryRequirements))
            {
                if (!HasOnlyFollowingValidCharacters(course.EntryRequirements))
                    validationMessages.Add("Entry Requirements contains invalid character");
                if (course.EntryRequirements.Length > _entryRequirementsTextFieldMaxChars)
                    validationMessages.Add($"Entry Requirements must be { _entryRequirementsTextFieldMaxChars } characters or less");
            }

            // WhatYoullLearn 
            if (!string.IsNullOrEmpty(course.WhatYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullLearn))
                    validationMessages.Add("What You'll Learn contains invalid character");
                if (course.WhatYoullLearn.Length > _whatWillLearnTextFieldMaxChars)
                    validationMessages.Add($"What You'll Learn must be { _whatWillLearnTextFieldMaxChars } characters or less");
            }

            // HowYoullLearn 
            if (!string.IsNullOrEmpty(course.HowYoullLearn))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullLearn))
                    validationMessages.Add("How You'll Learn contains invalid character");
                if (course.HowYoullLearn.Length > _howYouWillLearnTextFieldMaxChars)
                    validationMessages.Add($"How You'll Learn must be { _howYouWillLearnTextFieldMaxChars } characters or less");
            }

            // WhatYoullNeed 
            if (!string.IsNullOrEmpty(course.WhatYoullNeed))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhatYoullNeed))
                    validationMessages.Add("What You'll Need contains invalid character");
                if (course.WhatYoullNeed.Length > _whatYouNeedTextFieldMaxChars)
                    validationMessages.Add($"What You'll Need must be { _whatYouNeedTextFieldMaxChars } characters or less");
            }

            // HowYoullBeAssessed 
            if (!string.IsNullOrEmpty(course.HowYoullBeAssessed))
            {
                if (!HasOnlyFollowingValidCharacters(course.HowYoullBeAssessed))
                    validationMessages.Add("How You'll Be Assessed contains invalid character");
                if (course.HowYoullBeAssessed.Length > _howAssessedTextFieldMaxChars)
                    validationMessages.Add($"How You'll Be Assessed must be { _howAssessedTextFieldMaxChars } characters or less");
            }

            // WhereNext 
            if (!string.IsNullOrEmpty(course.WhereNext))
            {
                if (!HasOnlyFollowingValidCharacters(course.WhereNext))
                    validationMessages.Add("Where Next contains invalid character");
                if (course.WhereNext.Length > _whereNextTextFieldMaxChars)
                    validationMessages.Add($"Where Next must be { _whereNextTextFieldMaxChars } characters or less");
            }

            return validationMessages;
        }

        public IList<string> ValidateCourseRun(ICourseRun courseRun, ValidationMode validationMode)
        {
            var validationMessages = new List<string>();

            // CourseName
            if (string.IsNullOrEmpty(courseRun.CourseName))
            {
                validationMessages.Add("Course Name is required"); // "Enter Course Name"
            }
            else
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CourseName))
                    validationMessages.Add("Course Name contains invalid character");
                if (courseRun.CourseName.Length > 255)
                    validationMessages.Add($"Course Name must be 255 characters or less");
            }

            // ProviderCourseID
            if (!string.IsNullOrEmpty(courseRun.ProviderCourseID))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.ProviderCourseID))
                    validationMessages.Add("ID contains invalid characters");
                if (courseRun.ProviderCourseID.Length > 255)
                    validationMessages.Add($"The maximum length of 'ID' is 255 characters");
            }

            // DeliveryMode
            switch (courseRun.DeliveryMode)
            {
                case DeliveryMode.ClassroomBased:

                    // VenueId
                    if (courseRun.VenueId == null || courseRun.VenueId == Guid.Empty)
                        validationMessages.Add($"Select a venue");

                    // StudyMode
                    if (courseRun.StudyMode.Equals(StudyMode.Undefined))
                        validationMessages.Add($"Select Study Mode");

                    // AttendancePattern
                    if (courseRun.AttendancePattern.Equals(AttendancePattern.Undefined))
                        validationMessages.Add($"Select Attendance Pattern");

                    break;
                case DeliveryMode.Online:
                    // No Specific Fields
                    break;
                case DeliveryMode.WorkBased:

                    // Regions
                    if (courseRun.Regions == null || courseRun.Regions.Count().Equals(0))
                        validationMessages.Add($"Select a region");
                    break;
                case DeliveryMode.Undefined: // Question ???
                default:
                    validationMessages.Add($"DeliveryMode is Undefined. We are not checking the specific fields now. On editing you can select the appropriate Delivery Mode and the rest of the fields will be validated accordingly.");
                    break;
            }

            // StartDate & FlexibleStartDate
            if (courseRun.StartDate != null)
            {
                switch (validationMode)
                {
                    case ValidationMode.AddCourseRun:
                    case ValidationMode.CopyCourseRun:
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.EditCourseYC:
                    case ValidationMode.EditCourseMT:
                        // It cannot be done easily as we need both value - the newly entered and the previous. Call to saved version or modification in the model
                        break;
                    case ValidationMode.EditCourseBU:
                        // If the Provider does the editing on the same day of uploading it's fine. But from next day forward ?????????
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.BulkUploadCourse:
                        if (courseRun.StartDate < DateTime.Now || courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date cannot be before Today's Date and must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.MigrateCourse:
                        if (courseRun.StartDate > DateTime.Now.AddYears(2))
                            validationMessages.Add($"Start Date must be less than or equal to 2 years from Today's Date");
                        break;
                    case ValidationMode.Undefined: 
                    default:
                        validationMessages.Add($"Validation Mode was not defined.");
                        break;
                }
            }

            if (courseRun.StartDate == null && courseRun.FlexibleStartDate == false)
                validationMessages.Add($"Either 'Defined Start Date' or 'Flexible Start Date' has to be provided");

            // CourseURL
            if (!string.IsNullOrEmpty(courseRun.CourseURL))
            {
                if (!IsValidUrl(courseRun.CourseURL))
                    validationMessages.Add("The format of URL is incorrect");
                if (courseRun.CourseURL.Length > 255)
                    validationMessages.Add($"The maximum length of URL is 255 characters");
            }

            // Cost & CostDescription
            if (string.IsNullOrEmpty(courseRun.CostDescription) && courseRun.Cost.Equals(null))
                validationMessages.Add($"Enter cost or cost description");

            if (!string.IsNullOrEmpty(courseRun.CostDescription))
            {
                if (!HasOnlyFollowingValidCharacters(courseRun.CostDescription))
                    validationMessages.Add("Cost Description contains invalid characters");
                if (courseRun.CostDescription.Length > 255)
                    validationMessages.Add($"Cost description must be 255 characters or less");
            }

            if (!courseRun.Cost.Equals(null))
            {
                if (!IsCorrectCostFormatting(courseRun.Cost.ToString()))
                    validationMessages.Add($"Enter the cost in pounds and pence");
            }

            // DurationUnit
            if (courseRun.DurationUnit.Equals(DurationUnit.Undefined))
                validationMessages.Add($"Select Duration Unit");

            // DurationValue
            if (courseRun.DurationValue.Equals(null))
            {
                validationMessages.Add($"Enter Duration");
            }
            else
            {
                if (!ValidDurationValue(courseRun.DurationValue?.ToString()))
                    validationMessages.Add("Duration must be numeric and maximum length is 3 digits");
            }

            return validationMessages;
        }

        public bool HasOnlyFollowingValidCharacters(string value)
        {
            string regex = @"^[a-zA-Z0-9 /\n/\r/\¬\!\£\$\%\^\&\*\(\)_\+\-\=\{\}\[\]\;\:\@\'\#\~\,\<\>\.\?\/\|\`" + "\"" + "\\\\]+$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsValidUrl(string value)
        {
            string regex = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool IsCorrectCostFormatting(string value)
        {
            string regex = @"^[0-9]*(\.[0-9]{1,2})?$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public bool ValidDurationValue(string value)
        {
            string regex = @"^([0-9]|[0-9][0-9]|[0-9][0-9][0-9])$";
            var validUKPRN = Regex.Match(value, regex, RegexOptions.IgnoreCase);

            return validUKPRN.Success;
        }

        public async Task<IResult> ArchiveProviderLiveCourses(int? UKPRN)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));

            var response = await _httpClient.GetAsync(new Uri(_archiveLiveCoursesUri.AbsoluteUri + "&UKPRN=" + UKPRN));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();

            }
            else
            {
                return Result.Fail("Archive courses service unsuccessful http response");
            }
        }

        public async Task<IResult> ChangeCourseRunStatusesForUKPRNSelection(int UKPRN, int CurrentStatus, int StatusToBeChangedTo)
        {
            Throw.IfNull(UKPRN, nameof(UKPRN));
            Throw.IfNull(CurrentStatus, nameof(CurrentStatus));
            Throw.IfNull(StatusToBeChangedTo, nameof(StatusToBeChangedTo));

            var response = await _httpClient.GetAsync(new Uri(_changeCourseRunStatusesForUKPRNSelectionUri.AbsoluteUri + "&UKPRN=" + UKPRN + "&CurrentStatus=" + CurrentStatus + "&StatusToBeChangedTo=" + StatusToBeChangedTo));
            _logger.LogHttpResponseMessage("Archive courses service http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();
            }
            else
            {
                return Result.Fail("ChangeCourseRunStatusesForUKPRNSelection service unsuccessful http response");
            }
        }

        public async Task<IResult> UpdateStatus(Guid courseId, Guid courseRunId, int statusToUpdateTo)
        {
            Throw.IfNullGuid(courseId, nameof(courseId));
            Throw.IfLessThan(0, statusToUpdateTo, nameof(statusToUpdateTo));
            Throw.IfGreaterThan(Enum.GetValues(typeof(RecordStatus)).Cast<int>().Max(), statusToUpdateTo, nameof(statusToUpdateTo));

            var response = await _httpClient.GetAsync(new Uri(_updateStatusUri.AbsoluteUri 
                + "&CourseId=" + courseId
                + "&CourseRunId=" + courseRunId
                + "&Status=" + statusToUpdateTo));
            _logger.LogHttpResponseMessage("Update Status http response", response);

            if (response.IsSuccessStatusCode)
            {
                return Result.Ok();

            }
            else
            {
                return Result.Fail("Update course unsuccessful http response");
            }
        }
    }

    internal static class IGetCourseByIdCriteriaExtensions
    {
        internal static string ToJson(this IGetCourseByIdCriteria extendee)
        {

            GetCourseByIdJson json = new GetCourseByIdJson
            {
                id = extendee.Id.ToString()
            };
            var result = JsonConvert.SerializeObject(json);

            return result;
        }
    }

    internal class GetCourseByIdJson
    {
        public string id { get; set; }
    }

    internal static class CourseServiceSettingsExtensions
    {
        internal static Uri ToAddCourseUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "AddCourse?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetYourCoursesUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCoursesByLevelForUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToUpdateCourseUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateCourse?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetCourseByIdUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseById?code=" + extendee.ApiKey}");
        }
        internal static Uri ToArchiveLiveCoursesUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "ArchiveProvidersLiveCourses?code=" + extendee.ApiKey}");
        }
        internal static Uri ToUpdateStatusUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "UpdateStatus?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetCourseCountsByStatusForUKPRNUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetCourseCountsByStatusForUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToGetRecentCourseChangesByUKPRNUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "GetRecentCourseChangesByUKPRN?code=" + extendee.ApiKey}");
        }
        internal static Uri ToChangeCourseRunStatusesForUKPRNSelectionUri(this ICourseServiceSettings extendee)
        {
            return new Uri($"{extendee.ApiUrl + "ChangeCourseRunStatusesForUKPRNSelection?code=" + extendee.ApiKey}");
        }
    }
}
