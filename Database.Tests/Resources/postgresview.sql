CREATE VIEW jobcompanylocation AS
SELECT job.id, job.revision, job.title, job."date", job.expiration, job.reference, job."url", job.publisherid, job.companyid, job.locationid, job."description", job.requirements, job.offers, job.descriptiontype, job.salary, job.jobtype, job.experience, job.categories, job.education,
	publisher."name" as publishername, publisher."url" as publisherurl,
	company."name" as companyname, company."url" as companyurl,
	companyaddress.country, companyaddress."state", companyaddress.city, companyaddress.postalcode, companyaddress.line
FROM job
JOIN companyaddress on companyaddress.id=job.locationid
LEFT JOIN company as publisher on publisher.id=job.publisherid
LEFT JOIN company on company.id=job.companyid